using Cysharp.Threading.Tasks;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using Positron.Client.Room.Models;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Positron.Client.ConstantHolders;
using Positron.Client.Settings;
using Positron.Client.Mono;
using Positron.Client.Handlers;
using Positron.NetworkIoAPI;
using Positron.Client.Room.Models.Interfaces;

namespace Positron.Client.Room
{
    public class NetworkWorld : IDisposable
    {
        private NetworkClock _clock;
        private PositronNetworkWriter _writer;

        private IPositronClient _client;
        private RoomLeaveHandler _leaveHandler;
        private IPositronObservableHandler<JoinRoomResponse> _joinHandler;
        private IPositronObservableHandler<GameTickDataAndMeta> _gameTickHandler;
        private IPositronObservableHandler<GameUnreliableTick> _unreliableTickHandler;
        
        private CancellationTokenSource _ctx;

        private NetworkGameObjectsModel _gameObjectsModel;
        private NetworkValuesModel _valuesModel;
        private RpcsModel _rpcsModel;

        private JoinRoomResponse _joinDataPacket;
        private Action _loadCompleteCallback;
        private LoadSceneOverrider DoLoadScene;
        private Action DoLoadMainMenuScene;

        private int _tickRate;

        public uint HostId { get; private set; }
        public uint LocalClientId { get; private set; }
        public bool InRoom { get; private set; }
        public double NetworkTime => _clock.ClientTimeWithPastOffset;
        public ISendOnlyRpcModel RpcModel => _rpcsModel;

        public event Action hostChanged;
        public event Action roomLeaved;

        public NetworkWorld(PositronSettings settings, IPositronSerializer serializer)
        {
            _gameObjectsModel = new(this, settings);
            _valuesModel = new();
            _rpcsModel = new(settings);
            _clock = new(settings.TickOffset, settings.Tickrate);
            _writer = new(serializer);
        }

        public void Init(
            IPositronClient client,
            RoomLeaveHandler leaveRoomHandler,
            IPositronObservableHandler<JoinRoomResponse> joinHandler,
            IPositronObservableHandler<GameTickDataAndMeta> gameTickHandler,
            IPositronObservableHandler<GameUnreliableTick> unreliableTickHandler)
        {
            _client = client;
            _leaveHandler = leaveRoomHandler;
            _joinHandler = joinHandler;
            _gameTickHandler = gameTickHandler;
            _unreliableTickHandler = unreliableTickHandler;
            _ctx = new();

            _joinHandler.callback += Join;
            _leaveHandler.leaveConfirmed += Leave;
            _gameTickHandler.callback += ProcessReliableTickPacket;
            _unreliableTickHandler.callback += ProcessUnreliableTickPacket;
        }

        public void Dispose()
        {
            if (!InRoom)
            {
                return;
            }

            Leave();
            UnsubCompleteJoin();

            _clock.Dispose();
            _gameObjectsModel.Dispose();
            _valuesModel.Dispose();
            _rpcsModel.Dispose();

            _joinHandler.callback -= Join;
            _leaveHandler.leaveConfirmed -= Leave;
            _gameTickHandler.callback -= ProcessReliableTickPacket;
            _unreliableTickHandler.callback -= ProcessUnreliableTickPacket;
        }
        
        public void OverrideSceneLoader(LoadSceneOverrider sceneLoadFunc)
        {
            DoLoadScene = sceneLoadFunc;
        }

        public void OverrideLoadMainMenu(Action mainMenuLoadFunc)
        {
            DoLoadMainMenuScene = mainMenuLoadFunc;
        }

        public void Leave() 
        {
            if (!InRoom)
            {
                throw new InvalidOperationException("Critical error -> can`t leave outside room");
            }

            _clock.StopTime();
            _gameObjectsModel.ClearWorld();
            _valuesModel.ClearWorld();

            InRoom = false;
            _ctx.Cancel();
            _ctx.Dispose();

            roomLeaved?.Invoke();

            if (DoLoadMainMenuScene == null)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                DoLoadMainMenuScene();
            }
        }

        public void SpawnObject(PositronNetworkIdentity prefab, Vector3 position, Quaternion rotation)
        {
            if (!InRoom)
            {
                throw new InvalidOperationException("Critical error -> can`t create object not in room");
            }

            _gameObjectsModel.CreateLocalObjectAndSendToServer(prefab, position, rotation);
        }
            
        public void Destroy(PositronNetworkIdentity instance)
        {
            if (!InRoom)
            {
                throw new InvalidOperationException("Critical error -> can`t destroy object not in room");
            }

            _gameObjectsModel.DeleteObjectAndSendToServer(instance);
        }

        public void RequestOwnershipOn(PositronNetworkIdentity instance)
        {
            if (!InRoom)
            {
                throw new InvalidOperationException("Critical error -> can`t request ownership on object not in room");
            }

            _gameObjectsModel.RequestOwnership(instance);
        }

        public double TickToSeconds(uint tick) => _clock.TickToSeconds(tick);

        private void Join(JoinRoomResponse dataPacket)
        {
            if (InRoom)
            {
                throw new InvalidOperationException("Critical error -> can`t join another room");
            }

            _ctx = new();

            _clock.Reset();
            _joinDataPacket = dataPacket;

            if (_joinDataPacket.Scene == 0)
            {
                Debug.LogError("Unable to load boot scene via positron!");
            }

            if (DoLoadScene == null)
            {
                SceneManager.LoadScene((int)dataPacket.Scene);

#pragma warning disable UDR0005 // Domain Reload Analyzer
                SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore UDR0005 // Domain Reload Analyzer

                Debug.LogWarning("Positron uses own scene load fallback");
            }
            else
            {
                _loadCompleteCallback = DoLoadScene(dataPacket.Scene);
                _loadCompleteCallback += CompleteJoin;
            }
        }

        private void ProcessReliableTickPacket(GameTickDataAndMeta tickPacket)
        {
            if (HostId != tickPacket.Meta.Host)
            {
                HostId = tickPacket.Meta.Host;
                hostChanged?.Invoke();
            }

            _clock.TryInitTime(tickPacket.Meta.Tick);
            _clock.UpdateServerTime(tickPacket.Meta.Tick);
            _valuesModel.PutArenaFromServer(tickPacket.ValuesArena);

            _gameObjectsModel.CreateObjects(tickPacket.Meta.NewGameObjects, false);
            _valuesModel.PerformAddition(tickPacket.Meta.NewValues);
            _valuesModel.PerformModification(tickPacket.Meta.ModValues);
            _rpcsModel.ProcessServerRpcEvents(tickPacket.Meta.Rpcs, tickPacket.RpcsArena);

            _gameObjectsModel.RemoveObjects(tickPacket.Meta.RemovedObjects);

            _gameObjectsModel.TransferObjects(tickPacket.Meta.TransferedObjects);
        }

        private void ProcessUnreliableTickPacket(GameUnreliableTick unreliableTickPaclet)
        {
            _gameObjectsModel.MoveObjects(unreliableTickPaclet.MovedObjects, unreliableTickPaclet.Tick);
        }

        private async UniTask Tick()
        {
            await UniTask.SwitchToMainThread();

            TimeSpan tickInterval = TimeSpan.FromMilliseconds(1000d / ((double)_tickRate));

            while (InRoom)
            {
                await UniTask.Delay(tickInterval, delayType: DelayType.Realtime, delayTiming: PlayerLoopTiming.EarlyUpdate, cancellationToken: _ctx.Token);

                GameTickPacket tickPacket = new();
                tickPacket.Host = HostId;
                tickPacket.Client = LocalClientId;

                NetworkGameObjectsModel.GameObjectsDelta reliableGameObjectsDelta = _gameObjectsModel.GetActionsDelta();

                tickPacket.NewGameObjects = reliableGameObjectsDelta.NewGameOgjects;
                tickPacket.RemovedObjects = reliableGameObjectsDelta.RemovedGameObjectIds;
                tickPacket.TransferedObjects = reliableGameObjectsDelta.RequestOwnershipDelta;

                tickPacket.NewValues = _valuesModel.GetValuesAddDelta();
                tickPacket.ModValues = _valuesModel.GetValuesModDelta();
                tickPacket.Rpcs = _rpcsModel.GetCurrentDelta();

                _writer.Clear();
                _writer.WriteComplexObject(tickPacket);
                _writer.WriteBytes(_valuesModel.GetArena());
                _writer.WriteBytes(_rpcsModel.GetArena());
                _client.SendRaw(_writer.Data, EventTypes.TICK, true);


                GameUnreliableTick unreliableTick = new();
                unreliableTick.ClientId = LocalClientId;

                _gameObjectsModel.CollectCurrentObjectsMoveDeltas();
                _gameObjectsModel.CollectCurrentObjectsStaticDeltaIfRequired();
                unreliableTick.MovedObjects = _gameObjectsModel.GetMoveDelta();

                _client.Send(unreliableTick, EventTypes.UNRELIABLE_TICK, false);

                _gameObjectsModel.ClearDelta();
                _valuesModel.ClearDelta();
                _rpcsModel.ClearDelta();
            }
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            CompleteJoin();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void CompleteJoin()
        {
            InRoom = true;

            _tickRate = (int)_joinDataPacket.Tickrate;
            LocalClientId = _joinDataPacket.SelfId;
            HostId = _joinDataPacket.Host;
            _rpcsModel.Init(LocalClientId, _gameObjectsModel);
            _valuesModel.Init(_gameObjectsModel);

            _gameObjectsModel.CreateObjects(_joinDataPacket.GameObjects, true);
            
            _valuesModel.PutArenaFromServer(_joinDataPacket.NetValuesDataArena);
            _valuesModel.PerformAddition(_joinDataPacket.Values);

            _gameObjectsModel.WakeAllObjectsAfterSilentCreation();

            _rpcsModel.SetRollBufferMode(true);
            _rpcsModel.ProcessServerRpcEvents(_joinDataPacket.CachedRpcCalls, _joinDataPacket.RpcsDataArena);
            _rpcsModel.SetRollBufferMode(false);

            _gameObjectsModel.SetAllObjectsAvailable();

            Tick().Forget();

            UnsubCompleteJoin();
        }

        private void UnsubCompleteJoin()
        {
            if (_loadCompleteCallback != null)
            {
                _loadCompleteCallback -= CompleteJoin;
                _loadCompleteCallback = null;
            }
        }

        public delegate Action LoadSceneOverrider(uint scene);
    }
}
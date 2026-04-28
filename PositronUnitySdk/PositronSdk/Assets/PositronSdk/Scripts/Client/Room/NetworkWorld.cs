using Cysharp.Threading.Tasks;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using Positron.Client.Room.Models;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Positron.Client.Room
{
    public class NetworkWorld : IDisposable
    {
        private IPositronClient _client;
        private IPositronObservableHandler<JoinRoomResponse> _joinHandler;
        private CancellationTokenSource _ctx;

        private NetworkGameObjectsModel _gameObjectsModel;
        private NetworkValuesModel _valuesModel;
        private RpcsModel _rpcsModel;

        private JoinRoomResponse _joinDataPacket;
        private Action _loadCompleteCallback;
        private LoadSceneOverrider DoLoadScene;

        private int _tickRate;

        public uint HostId { get; private set; }
        public uint LocalClientId { get; private set; }
        public bool InRoom { get; private set; }

        public NetworkWorld()
        {
            _gameObjectsModel = new(this);
            _valuesModel = new();
            _rpcsModel = new();
        }

        public void Init(IPositronClient client, IPositronObservableHandler<JoinRoomResponse> joinHandler)
        {
            _client = client;
            _joinHandler = joinHandler;
            _ctx = new();

            _joinHandler.callback += Join;
        }

        public void Dispose()
        {
            if (!InRoom)
            {
                return;
            }

            Leave();
            UnsubCompleteJoin();

            _gameObjectsModel.Dispose();
            _valuesModel.Dispose();
            _rpcsModel.Dispose();

            _joinHandler.callback -= Join;
        }

        public void OverrideSceneLoader(LoadSceneOverrider sceneLoadFunc)
        {
            DoLoadScene = sceneLoadFunc;
        }

        public void Leave() 
        {
            if (!InRoom)
            {
                Debug.LogError("Critical error -> can`t leave outside room");
                return;
            }

            _gameObjectsModel.ClearWorld();
            _valuesModel.ClearWorld();

            InRoom = false;
            _ctx.Cancel();
            _ctx.Dispose();
        }

        private void Join(JoinRoomResponse dataPacket)
        {
            if (InRoom)
            {
                Debug.LogError("Critical error -> can`t join another room");
                return;
            }

            _joinDataPacket = dataPacket;

            if (_joinDataPacket.Scene == 0)
            {
                Debug.LogError("Unable to load boot scene via positron!");
            }

            if (DoLoadScene == null)
            {
                SceneManager.LoadScene((int)dataPacket.Scene);
                Debug.LogWarning("Positron uses own scene load fallback");

                CompleteJoin();
            }
            else
            {
                _loadCompleteCallback = DoLoadScene(dataPacket.Scene);
                _loadCompleteCallback += CompleteJoin;
            }
        }

        private void ProcessReliableTickPacket(GameTickPacket tickPacket)
        {
            _gameObjectsModel.CreateObjects(tickPacket.NewGameObjects);
            _gameObjectsModel.RemoveObjects(tickPacket.RemovedObjects);
            _gameObjectsModel.TransferedObjects(tickPacket.TransferedToHostObjects, tickPacket.Host);

            _valuesModel.AddOrModifyValues(tickPacket.ValueModification);

            _rpcsModel.MultiCall(tickPacket.Rpcs);
        }

        private void ProcessUnreliableTickPacket(GameUnreliableTick unreliableTickPaclet)
        {
            _gameObjectsModel.MoveObjects(unreliableTickPaclet.MovedObjects);
        }

        private async UniTask Tick()
        {
            await UniTask.SwitchToMainThread();

            while (InRoom)
            {
                await UniTask.Delay(1000 / _tickRate, cancellationToken: _ctx.Token, delayTiming: PlayerLoopTiming.FixedUpdate);
            
                // collect data
                // send to server
            }
        }

        private void CompleteJoin()
        {
            _tickRate = (int)_joinDataPacket.Tickrate;
            LocalClientId = _joinDataPacket.SelfId;
            HostId = _joinDataPacket.Host;

            _gameObjectsModel.CreateObjects(_joinDataPacket.GameObjects);
            _valuesModel.AddOrModifyValues(_joinDataPacket.Values);
            _rpcsModel.MultiCall(_joinDataPacket.CachedRpcCalls);

            InRoom = true;
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
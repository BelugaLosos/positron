using Cysharp.Threading.Tasks;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using Positron.Client.Room.Models;
using System;
using System.Threading;
using UnityEngine;

namespace Positron.Client.Room
{
    public class NetworkWorld : IDisposable
    {
        private IPositronClient _client;
        private CancellationTokenSource _ctx;

        private NetworkGameObjectsModel _gameObjectsModel;
        private NetworkValuesModel _valuesModel;
        private RpcsModel _rpcsModel;

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

        public void Init(IPositronClient client)
        {
            _client = client;
            _ctx = new();
        }

        public void Dispose()
        {
            if (!InRoom)
            {
                return;
            }

            Leave();

            _gameObjectsModel.Dispose();
            _valuesModel.Dispose();
            _rpcsModel.Dispose();
        }

        public void Join(JoinRoomResponse dataPacket)
        {
            if (InRoom)
            {
                Debug.LogError("Critical error -> can`t join another room");
                return;
            }

            _tickRate = (int)dataPacket.Tickrate;
            LocalClientId = dataPacket.SelfId;
            HostId = dataPacket.Host;

            _gameObjectsModel.CreateObjects(dataPacket.GameObjects);
            _valuesModel.AddOrModifyValues(dataPacket.Values);
            _rpcsModel.MultiCall(dataPacket.CachedRpcCalls);

            InRoom = true;
            Tick().Forget();
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

        public void ProcessReliableTickPacket(GameTickPacket tickPacket)
        {
            _gameObjectsModel.CreateObjects(tickPacket.NewGameObjects);
            _gameObjectsModel.RemoveObjects(tickPacket.RemovedObjects);
            _gameObjectsModel.TransferedObjects(tickPacket.TransferedToHostObjects, tickPacket.Host);

            _valuesModel.AddOrModifyValues(tickPacket.ValueModification);

            _rpcsModel.MultiCall(tickPacket.Rpcs);
        }

        public void ProcessUnreliableTickPacket(GameUnreliableTick unreliableTickPaclet)
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
    }
}
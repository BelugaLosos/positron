using Cysharp.Threading.Tasks;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using System;
using System.Threading;
using UnityEngine;

namespace Positron.Client.Room
{
    public class NetworkWorld : IDisposable
    {
        private IPositronClient _client;
        private CancellationTokenSource _ctx;

        public bool InRoom { get; private set; }

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
        }

        public void Join(JoinRoomResponse dataPacket)
        {
            if (InRoom)
            {
                Debug.LogError("Critical error -> can`t join another room");
                return;
            }

            // init world, load scene e.g.

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

            InRoom = false;
            _ctx.Cancel();
            _ctx.Dispose();
        }

        public void ProcessReliableTickPacket(GameTickPacket tickPacket)
        {
            // process world change
            // OBJ
            // VAL
            // RPC
        }

        public void ProcessUnreliableTickPacket(GameUnreliableTick unreliableTickPaclet)
        {
            // process move
            // check existance
            // move
        }

        private async UniTask Tick()
        {
            await UniTask.SwitchToMainThread();

            while (InRoom)
            {
                await UniTask.Delay(1000 / _client.Settings.Tickrate, cancellationToken: _ctx.Token, delayTiming: PlayerLoopTiming.FixedUpdate);
            
                // collect data
                // send to server
            }
        }
    }
}
using Cysharp.Threading.Tasks;
using Positron.Client.Interfaces;
using UnityEngine;
using Positron.Client.ConstantHolders;
using System;
using System.Threading;

namespace Positron.Client.Ping
{
    public class PingModel : IReadOnlyPingModel, IDisposable
    {
        private IPositronClient _client;
        private double _pingTime;
        private double _pongTime;

        private CancellationTokenSource _ctx;

        public int LatencyMs { get; private set; }

        public event Action estimated;

        public void Init(IPositronClient client)
        {
            _client = client;
            _ctx = new();

            EstimationLoop().Forget();
        }

        public void Pong()
        {
            _pongTime = Time.timeAsDouble;
            LatencyMs = Mathf.RoundToInt((float)TimeSpan.FromSeconds(_pongTime - _pingTime).TotalMilliseconds);
            
            estimated?.Invoke();
        }

        public async UniTask EstimationLoop()
        {
            await UniTask.SwitchToMainThread();
            await UniTask.WaitUntil(() => _client.Status == ClientStatus.Connected);

            while (_client.Status == ClientStatus.Connected)
            {
                _client.SendRaw(stackalloc byte[] { 0xFF }, EventTypes.PING, true);
                _pingTime = Time.timeAsDouble;
                await UniTask.WaitForSeconds(1f, cancellationToken: _ctx.Token);
            }
        }

        public void Dispose()
        {
            if (_ctx == null)
            {
                return;
            }

            _ctx.Cancel();
            _ctx.Dispose();
        
            _ctx = null;
        }
    }
}
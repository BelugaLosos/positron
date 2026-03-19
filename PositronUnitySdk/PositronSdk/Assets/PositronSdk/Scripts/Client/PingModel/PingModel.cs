using Cysharp.Threading.Tasks;
using Positron.Client.Interfaces;
using UnityEngine;
using Positron.Client.ConstantHolders;
using System;

namespace Positron.Client.Ping
{
    public class PingModel : IReadOnlyPingModel
    {
        private IPositronClient _client;
        private double _pingTime;
        private double _pongTime;

        public int LatencyMs { get; private set; }

        public event Action estimated;

        public void Init(IPositronClient client)
        {
            _client = client;
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
                await UniTask.WaitForSeconds(1f);
            }
        }
    }
}
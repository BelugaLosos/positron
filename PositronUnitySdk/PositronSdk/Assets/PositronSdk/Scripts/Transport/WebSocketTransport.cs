using Cysharp.Threading.Tasks;
using NativeWebSocket;
using Positron.Client.Interfaces;
using Positron.Client.Settings;
using System;
using System.Threading;

namespace Positron.Transport
{
    public sealed class WebSocketTransport : IPositronTransport
    {
        private WebSocket _webSokcet;
        private CancellationTokenSource _dispatchCancellationToken;

        public event Action<byte[]> onMessage;

        public async UniTask Connect(PositronSettings settings)
        {
            await UniTask.SwitchToMainThread();

            _dispatchCancellationToken = new();
            _webSokcet = new($"{(settings.IsSecure ? "wss" : "ws")}://{settings.Address}:{settings.Port}");

            await _webSokcet.Connect();

            DispathLoop().Forget();

            _webSokcet.OnMessage += (data) =>
            {
                onMessage?.Invoke(data);
            };
        }

        public async UniTask Disconnect()
        {
            await UniTask.SwitchToMainThread();

            if (_webSokcet.State == WebSocketState.Open)
            {
                await _webSokcet.Close();
            }

            _dispatchCancellationToken.Cancel();
        }

        public void Send(Span<byte> buffer)
        {
            if (_webSokcet.State != WebSocketState.Open)
            {
                return;
            }

            _webSokcet.Send(buffer.ToArray());
        }

        private async UniTask DispathLoop()
        {
            await UniTask.SwitchToMainThread();

            while (_webSokcet.State == WebSocketState.Open)
            {
                _webSokcet.DispatchMessageQueue();
                await UniTask.Yield(cancellationToken: _dispatchCancellationToken.Token);
            }
        }
    }
}
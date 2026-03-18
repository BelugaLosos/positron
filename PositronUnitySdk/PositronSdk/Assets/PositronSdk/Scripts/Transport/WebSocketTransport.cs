using Cysharp.Threading.Tasks;
using NativeWebSocket;
using Positron.Client.ConstantHolders;
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

        public event Action<EventTypes, byte[]> onRawMessage;

        public async UniTask Connect(PositronSettings settings)
        {
            await UniTask.SwitchToMainThread();

            _dispatchCancellationToken = new();
            _webSokcet = new($"{(settings.IsSecure ? "wss" : "ws")}://{settings.Address}:{settings.Port}");

            await _webSokcet.Connect();

            DispathLoop().Forget();

            _webSokcet.OnMessage += (data) =>
            {
                Span<byte> packet = data;
                EventTypes type = (EventTypes)packet[0];
                bool isCompressed = data[1] == 1;

                Span<byte> payload = packet.Slice(2);

                if (isCompressed)
                {
                    payload = lz4.Decompress(payload.ToArray());
                }

                onRawMessage?.Invoke(type, payload.ToArray());
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

        public void Send(Span<byte> rawMessage, EventTypes type, bool isReliable)
        {
            if (_webSokcet.State != WebSocketState.Open)
            {
                return;
            }

            byte isCompressed = 0;
            Span<byte> resultiveMessage;

            if (rawMessage.Length > 1000)
            {
                resultiveMessage = lz4.Compress(rawMessage.ToArray());
                isCompressed = 1;
            }
            else
            {
                resultiveMessage = rawMessage;
            }

            Span<byte> socketMessage = stackalloc byte[2 + resultiveMessage.Length];

            socketMessage[0] = (byte)type;
            socketMessage[1] = isCompressed;

            resultiveMessage.CopyTo(socketMessage.Slice(2));

            _webSokcet.Send(socketMessage.ToArray());
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
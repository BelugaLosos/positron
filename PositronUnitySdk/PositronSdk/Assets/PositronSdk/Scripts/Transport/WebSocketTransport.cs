using Cysharp.Threading.Tasks;
using K4os.Compression.LZ4;
using NativeWebSocket;
using Positron.Client.ConstantHolders;
using Positron.Client.Interfaces;
using Positron.Client.Settings;
using System;
using System.Buffers;
using System.Threading;
using UnityEngine;

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

            if (_dispatchCancellationToken != null)
            {
                _dispatchCancellationToken.Cancel();
                _dispatchCancellationToken.Dispose();
            }

            _dispatchCancellationToken = new();
            _webSokcet = new($"{(settings.IsSecure ? "wss" : "ws")}://{settings.Address}:{settings.Port}");

            UniTaskCompletionSource connectTcs = new();

            _webSokcet.OnOpen += () => connectTcs.TrySetResult();
            _webSokcet.OnError += (e) => connectTcs.TrySetException(new(e));
            _webSokcet.OnClose += (e) => 
            {
                _dispatchCancellationToken?.Cancel(); 
            };

            _webSokcet.OnMessage += (data) =>
            {
                Span<byte> packet = data;
                EventTypes type = (EventTypes)packet[0];
                bool isCompressed = data[1] == 1;

                Span<byte> payload = packet.Slice(2);

                if (isCompressed)
                {
                    byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(ushort.MaxValue * 2); // THIS SHIT MUST BE RPLACED TO SERVER-SIDE PACKET SIZE OF SOURCE DATA

                    try
                    {
                        Span<byte> decompressBuffer = sharedBuffer;
                        int readedAmount = LZ4Codec.Decode(payload, decompressBuffer);

                        if (readedAmount <= 0)
                        {
                            throw new Exception("LZ4Decompress fault");
                        }

                        payload = decompressBuffer.Slice(0, readedAmount);

                        onRawMessage?.Invoke(type, payload.ToArray()); // this data can be copied by pointers deeper in call chain. It is MUST BE ALLOCATED IN COPY!
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(sharedBuffer);
                    }
                }
                else
                {
                    onRawMessage?.Invoke(type, payload.ToArray());
                }
            };


            DispathLoop().Forget();
            _ = _webSokcet.Connect();

            await connectTcs.Task;
        }

        public async UniTask Disconnect()
        {
            await UniTask.SwitchToMainThread();

            if (_webSokcet.State == WebSocketState.Open)
            {
                await _webSokcet.Close();
            }
        }

        public void Send(Span<byte> rawMessage, EventTypes type, bool isReliable)
        {
            if (_webSokcet == null || _webSokcet.State != WebSocketState.Open)
            {
                return;
            }

            if (rawMessage.Length > 1000)
            {
                int maxLen = LZ4Codec.MaximumOutputSize(rawMessage.Length);
                byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(maxLen);
                byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(maxLen + 2);

                try
                {
                    Span<byte> compressionBuffer = new(sharedBuffer);
                    int compressedLength = LZ4Codec.Encode(rawMessage, compressionBuffer);
                    compressionBuffer = compressionBuffer.Slice(0, compressedLength);

                    Span<byte> socketMsg = new(sendBuffer);
                    socketMsg[0] = (byte)type;
                    socketMsg[1] = 1;
                    
                    compressionBuffer.CopyTo(socketMsg.Slice(2));

                    _webSokcet.Send(socketMsg.Slice(0, compressedLength + 2).ToArray()); // this shit does not allow usage of array segment or smth like that. IT FORCES ME TO DO ALLOC!!!
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(sharedBuffer);
                    ArrayPool<byte>.Shared.Return(sendBuffer);
                }
            }
            else
            {
                byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(rawMessage.Length + 2);

                try
                {
                    Span<byte> socketMsg = new(sendBuffer);
                    socketMsg[0] = (byte)type;
                    socketMsg[1] = 0;

                    rawMessage.CopyTo(socketMsg.Slice(2));

                    _webSokcet.Send(socketMsg.Slice(0, rawMessage.Length + 2).ToArray()); // this shit does not allow usage of array segment or smth like that. IT FORCES ME TO DO ALLOC!!!
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(sendBuffer);
                }
            }
        }

        private async UniTask DispathLoop()
        {
            await UniTask.WaitWhile(() => _webSokcet == null);

            while (!_dispatchCancellationToken.IsCancellationRequested)
            {
                if (_webSokcet == null || _webSokcet.State != WebSocketState.Open)
                {
                    break;
                }

                _webSokcet.DispatchMessageQueue();
                bool cancelled = await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _dispatchCancellationToken.Token).SuppressCancellationThrow();
                if (cancelled) break;
            }
        }
    }
}
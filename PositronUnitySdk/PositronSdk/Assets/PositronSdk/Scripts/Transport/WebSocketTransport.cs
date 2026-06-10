using Cysharp.Threading.Tasks;
using K4os.Compression.LZ4;
using NativeWebSocket;
using Positron.Client.ConstantHolders;
using Positron.Client.Interfaces;
using Positron.Client.Settings;
using Positron.Transport.Utility;
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
                PacketData packetData = PacketUtility.DeconstructPacket(data);

                if (packetData.IsCompressed)
                {
                    byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent((int)packetData.SourceSize);

                    try
                    {
                        Span<byte> decompressBuffer = sharedBuffer;
                        int readedAmount = LZ4Codec.Decode(packetData.Data, decompressBuffer);

                        if (readedAmount <= 0)
                        {
                            throw new Exception("LZ4Decompress fault");
                        }

                        packetData.Data = decompressBuffer.Slice(0, readedAmount);

                        onRawMessage?.Invoke(packetData.Event, packetData.Data.ToArray()); // this data can be copied by pointers deeper in call chain. It is MUST BE ALLOCATED IN COPY!
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(sharedBuffer);
                    }
                }
                else
                {
                    onRawMessage?.Invoke(packetData.Event, packetData.Data.ToArray());
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
                byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(maxLen + PacketUtility.PROTOCOL_HEADER_MAX_OFFSET);

                try
                {
                    Span<byte> compressionBuffer = new(sharedBuffer);
                    int compressedLength = LZ4Codec.Encode(rawMessage, compressionBuffer);
                    compressionBuffer = compressionBuffer.Slice(0, compressedLength);

                    Span<byte> constructedPacket = PacketUtility.GlueDataToOptions(type, true, (uint)rawMessage.Length, compressionBuffer, sendBuffer);
                    _webSokcet.Send(constructedPacket.ToArray()); // this shit does not allow usage of array segment or smth like that. IT FORCES ME TO DO ALLOC!!!
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
                byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(rawMessage.Length + PacketUtility.PROTOCOL_HEADER_MAX_OFFSET);

                try
                {
                    Span<byte> constructedPacket = PacketUtility.GlueDataToOptions(type, false, 0, rawMessage, sendBuffer);
                    _webSokcet.Send(constructedPacket.ToArray()); // this shit does not allow usage of array segment or smth like that. IT FORCES ME TO DO ALLOC!!!
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
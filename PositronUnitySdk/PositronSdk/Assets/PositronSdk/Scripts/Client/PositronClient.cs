using Cysharp.Threading.Tasks;
using Positron.Client.ConstantHolders;
using Positron.Client.Interfaces;
using Positron.Client.Settings;
using System;
using UnityEngine;

namespace Positron.Client
{
    public sealed class PositronClient : IDisposable, IPositronClient
    {
        private readonly PositronSettings _settings;
        private readonly IPositronSerializer _serializer;
        private readonly IPositronTransport _transport;
        private readonly IPositronHandler[] _handlers;

        public ClientStatus Status { get; private set; }

        public event Action connected;
        public event Action disconnected;

        public PositronClient(PositronSettings settings, IPositronSerializer serializer, IPositronTransport transport, params IPositronHandler[] handlers)
        {
            _settings = settings;
            _serializer = serializer;
            _transport = transport;
            _handlers = handlers;

            Status = ClientStatus.Disconnected;

            foreach (IPositronHandler handler in _handlers)
            {
                handler.Init(this);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Connect()
        {
            if (Status != ClientStatus.Disconnected)
            {
                return;
            }

            ConnectWithSettings().Forget();
        }

        public void Disconnect()
        {
            if (Status == ClientStatus.Connected)
            {
                DisconnectAsync().Forget();
            }
        }

        public void Send<T>(T data, EventTypes eventType, bool reliable)
        {
            _transport.Send(_serializer.Serialize(data), eventType, reliable);
        }

        public void SendRaw(Span<byte> payloadData, EventTypes eventType, bool reliable)
        {
            _transport.Send(payloadData, eventType, reliable);
        }

        private void OnReceiveMessageFromTransport(EventTypes types, byte[] payloadData)
        {
            foreach (IPositronHandler handler in _handlers)
            {
                if (handler.MessageType == types)
                {
                    handler.Handle(payloadData);
                    break;
                }
            }
        }

        private async UniTask DisconnectAsync()
        {
            await UniTask.SwitchToMainThread();

            Status = ClientStatus.Disconnecing;
            await _transport.Disconnect();
            Status = ClientStatus.Disconnected;

            disconnected?.Invoke();
        }

        private async UniTask ConnectWithSettings()
        {
            await UniTask.SwitchToMainThread();

            try
            {
                Status = ClientStatus.Connecting;
                await _transport.Connect(_settings);
                Status = ClientStatus.Connected;

                _transport.onRawMessage += OnReceiveMessageFromTransport;

                connected?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                disconnected?.Invoke();
            }
        }
    }

    public enum ClientStatus
    {
        Connecting,
        Disconnecing,
        Disconnected,
        Connected
    }
}
using Positron.Client.ConstantHolders;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using System;
using UnityEngine;

namespace Positron.Client.Handlers
{
    public class GameTickHandler : IPositronHandler, IPositronObservableHandler<GameTickPacket>
    {
        private IPositronClient _client;
        public EventTypes MessageType => EventTypes.TICK;

        public event Action<GameTickPacket> callback;

        public void Init(IPositronClient client)
        {
            _client = client;
        }

        public void Dispose() { }

        public void Handle(Span<byte> packet)
        {
            callback?.Invoke(_client.Serializer.Deserialize<GameTickPacket>(packet));
        }
    }
}
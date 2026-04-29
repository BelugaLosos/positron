using Positron.Client.ConstantHolders;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using System;

namespace Positron.Client.Handlers
{
    public class GameUnreliableTickHandler : IPositronHandler, IPositronObservableHandler<GameUnreliableTick>
    {
        private IPositronClient _client;
        public EventTypes MessageType => EventTypes.UNRELIABLE_TICK;

        public event Action<GameUnreliableTick> callback;

        public void Init(IPositronClient client)
        {
            _client = client;
        }

        public void Dispose() { }

        public void Handle(Span<byte> packet)
        {
            callback?.Invoke(_client.Serializer.Deserialize<GameUnreliableTick>(packet));
        }
    }
}

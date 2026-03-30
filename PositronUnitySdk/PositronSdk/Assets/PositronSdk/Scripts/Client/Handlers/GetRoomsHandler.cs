using Positron.Client.ConstantHolders;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using System;

namespace Positron.Client.Handlers
{
    public sealed class GetRoomsHandler : IPositronHandler, IPositronObservableHandler<RoomListResponse>
    {
        private IPositronClient _client;

        public EventTypes MessageType => EventTypes.ROOMS_LIST;

        public event Action<RoomListResponse> callback;

        public void Init(IPositronClient client) 
        { 
            _client = client;
        }

        public void Handle(Span<byte> packet)
        {
            callback?.Invoke(_client.Serializer.Deserialize<RoomListResponse>(packet));
        }
    }
}
using Positron.Client.ConstantHolders;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using System;

namespace Positron.Client.Handlers
{
    public class RoomJoinedHandler : IPositronHandler, IPositronObservableHandler<JoinRoomResponse>
    {
        private IPositronClient _client;
        public EventTypes MessageType => EventTypes.ROOM_JOINED;

        public event Action<JoinRoomResponse> callback;

        public void Init(IPositronClient client)
        {
            _client = client;
        }

        public void Dispose() { }

        public void Handle(Span<byte> packet)
        {
            callback?.Invoke(_client.Serializer.Deserialize<JoinRoomResponse>(packet));
        }
    }
}
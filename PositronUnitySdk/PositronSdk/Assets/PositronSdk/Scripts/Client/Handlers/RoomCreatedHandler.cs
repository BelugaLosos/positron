using Positron.Client.ConstantHolders;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using System;

namespace Positron.Client.Handlers
{
    public class RoomCreatedHandler : IPositronHandler, IPositronObservableHandler<RoomCreationResponse>
    {
        private IPositronClient _client;

        public EventTypes MessageType => EventTypes.ROOM_CREATED;

        public event Action<RoomCreationResponse> callback;

        public void Init(IPositronClient client)
        {
            _client = client;
        }

        public void Dispose() { }

        public void Handle(Span<byte> packet)
        {
            RoomCreationResponse response = _client.Serializer.Deserialize<RoomCreationResponse>(packet);   
            callback?.Invoke(response);
        }
    }
}
using Positron.Client.ConstantHolders;
using Positron.Client.Interfaces;
using System;

namespace Positron.Client.Handlers
{
    public class RoomLeaveHandler : IPositronHandler
    {
        public EventTypes MessageType => EventTypes.ROOM_DISCONNECTED;

        public event Action leaveConfirmed;

        public void Init(IPositronClient client) { }
        public void Dispose() { }

        public void Handle(Span<byte> packet)
        {
            leaveConfirmed?.Invoke();
        }
    }
}
using Positron.Client.ConstantHolders;
using Positron.Client.Interfaces;
using Positron.Client.Ping;
using System;

namespace Positron.Client.Handlers
{
    public class PingHandler : IPositronHandler
    {
        private readonly PingModel _pingModel;

        public EventTypes MessageType => EventTypes.PONG;
        public void Init(IPositronClient client) { }

        public PingHandler(PingModel pingModel)
        {
            _pingModel = pingModel;
        }

        public void Dispose()
        {
            _pingModel.Dispose();
        }

        public void Handle(Span<byte> packet)
        {
            _pingModel.Pong();
        }
    }
}
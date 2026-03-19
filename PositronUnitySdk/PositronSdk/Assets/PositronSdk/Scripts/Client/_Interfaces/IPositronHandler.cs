using Positron.Client.ConstantHolders;
using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronHandler
    {
        EventTypes MessageType { get; }
        void Init(IPositronClient client);
        void Handle(Span<byte> packet);
    }
}
using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronHandler
    {
        byte MessageType { get; }
        void Handle(Span<byte> packet);
    }
}
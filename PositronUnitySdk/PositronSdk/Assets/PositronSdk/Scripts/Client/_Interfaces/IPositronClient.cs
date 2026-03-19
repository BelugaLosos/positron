using Positron.Client.ConstantHolders;
using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronClient
    {
        ClientStatus Status { get; }
        void Send<T>(T data, EventTypes eventType, bool reliable);
        void SendRaw(Span<byte> payloadData, EventTypes eventType, bool reliable);
    }
}
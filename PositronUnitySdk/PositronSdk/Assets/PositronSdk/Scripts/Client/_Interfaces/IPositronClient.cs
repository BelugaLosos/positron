using Positron.Client.ConstantHolders;
using Positron.Client.Settings;
using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronClient
    {
        ClientStatus Status { get; }
        IPositronSerializer Serializer { get; }
        PositronSettings Settings { get; }
        void Send<T>(T data, EventTypes eventType, bool reliable);
        void SendRaw(Span<byte> payloadData, EventTypes eventType, bool reliable);
    }
}
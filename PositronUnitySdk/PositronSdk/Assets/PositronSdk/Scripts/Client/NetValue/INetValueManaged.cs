using System;

namespace Positron.Client.NetValues
{
    public interface INetValueManaged
    {
        bool IsModified { get; }
        event Action dataChanged;
        void SerializeSelfTo(Span<byte> container);
        void DeserializeSelfFrom(ReadOnlyMemory<byte> container);
    }
}
using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues
{
    public interface INetValueManaged
    {
        bool IsModified { get; }
        bool IsFullyInited { get; } 
        event Action dataChanged;
        void MarkInited();
        int SerializeSelfTo(Span<byte> container, IPositronSerializer serializer);
        void DeserializeSelfFrom(ReadOnlyMemory<byte> container, IPositronSerializer serializer);
    }
}
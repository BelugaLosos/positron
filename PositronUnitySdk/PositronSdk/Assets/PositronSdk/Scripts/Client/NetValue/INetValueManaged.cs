using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues
{
    public interface INetValueManaged
    {
        bool IsFullyInited { get; } 
        event Action<INetValueManaged, uint> dataChangedWithFullCallback;
        event Action changed;
        void MarkInited(uint flatArrayIdDescriptor);
        int SerializeSelfTo(Span<byte> container, IPositronSerializer serializer);
        void DeserializeSelfFrom(ReadOnlyMemory<byte> container, IPositronSerializer serializer);
    }
}
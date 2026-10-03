using Positron.Client.Interfaces;
using Positron.Client.Mono;
using System;

namespace Positron.Client.NetValues
{
    public interface INetValueManaged
    {
        bool IsFullyInited { get; } 
        NetValueAuthority Authority { get; }
        event Action<INetValueManaged, uint> dataChangedWithFullCallback;
        event Action changed;
        void MarkInited(uint flatArrayIdDescriptor);
        void BindNetworkObject(PositronNetworkIdentity identity, bool predictable, NetValueAuthority authority);
        int SerializeSelfTo(Span<byte> container, IPositronSerializer serializer);
        void DeserializeSelfFrom(ReadOnlyMemory<byte> container, IPositronSerializer serializer);
    }
}
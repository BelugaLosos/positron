using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;

namespace Positron.Client.NetValues.Implements
{
    public class IntNetValue : NetValueManagedBase<int>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt32BigEndian(container, _value);
            return 4;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BinaryPrimitives.ReadInt32BigEndian(container.Span);
        }
    }
}
using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;

namespace Positron.Client.NetValues.Implements
{
    public class LongNetValue : NetValueManagedBase<long>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt64BigEndian(container, _value);
            return 8;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BinaryPrimitives.ReadInt64BigEndian(container.Span);
        }
    }
}
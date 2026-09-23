using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;

namespace Positron.Client.NetValues.Implements
{
    public sealed class UlongNetValue : NetValueManagedBase<ulong>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteUInt64BigEndian(container, _value);
            return 8;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BinaryPrimitives.ReadUInt64BigEndian(container.Span);
        }
    }
}
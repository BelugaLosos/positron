using Positron.Client.Interfaces;
using System.Buffers.Binary;
using System;

namespace Positron.Client.NetValues.Implements
{
    public sealed class ShortNetValue : NetValueManagedBase<short>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt16BigEndian(container, _value);
            return 2;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BinaryPrimitives.ReadInt16BigEndian(container.Span);
        }
    }
}
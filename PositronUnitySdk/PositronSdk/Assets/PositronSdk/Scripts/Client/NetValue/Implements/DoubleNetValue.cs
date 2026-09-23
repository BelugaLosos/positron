using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;

namespace Positron.Client.NetValues.Implements
{
    public sealed class DoubleNetValue : NetValueManagedBase<double>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt64BigEndian(container, BitConverter.DoubleToInt64Bits(_value));

            return 8;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(container.Span));
        }
    }
}
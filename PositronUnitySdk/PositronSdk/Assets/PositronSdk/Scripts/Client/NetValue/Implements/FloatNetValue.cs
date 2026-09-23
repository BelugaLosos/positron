using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;

namespace Positron.Client.NetValues.Implements
{
    public class FloatNetValue : NetValueManagedBase<float>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt32BigEndian(container, BitConverter.SingleToInt32Bits(_value));
            return 4;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(container.Span));
        }
    }
}
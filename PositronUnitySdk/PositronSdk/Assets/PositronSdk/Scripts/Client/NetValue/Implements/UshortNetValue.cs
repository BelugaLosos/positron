using Positron.Client.Interfaces;
using System.Buffers.Binary;
using System;

namespace Positron.Client.NetValues.Implements
{
    public class UshortNetValue : NetValueManagedBase<ushort>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteUInt16BigEndian(container, _value);
            return 2;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BinaryPrimitives.ReadUInt16BigEndian(container.Span);
        }
    }
}
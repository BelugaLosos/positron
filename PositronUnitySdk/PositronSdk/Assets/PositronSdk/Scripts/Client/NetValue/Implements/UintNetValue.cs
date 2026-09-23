using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;

namespace Positron.Client.NetValues.Implements
{
    public class UintNetValue : NetValueManagedBase<uint>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteUInt32BigEndian(container, _value);
            return 4;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = BinaryPrimitives.ReadUInt32BigEndian(container.Span);
        }
    }
}
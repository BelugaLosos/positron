using Positron.Client.Interfaces;
using System.Buffers.Binary;
using System;
using UnityEngine;

namespace Positron.Client.NetValues.Implements
{
    public sealed class Vector3NetValue : NetValueManagedBase<Vector3>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt32BigEndian(container, BitConverter.SingleToInt32Bits(_value.x));
            BinaryPrimitives.WriteInt32BigEndian(container.Slice(4), BitConverter.SingleToInt32Bits(_value.y));
            BinaryPrimitives.WriteInt32BigEndian(container.Slice(8), BitConverter.SingleToInt32Bits(_value.z));

            return 12;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value.x = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(container.Span));
            _value.y = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(container.Slice(4).Span));
            _value.z = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(container.Slice(8).Span));
        }
    }
}
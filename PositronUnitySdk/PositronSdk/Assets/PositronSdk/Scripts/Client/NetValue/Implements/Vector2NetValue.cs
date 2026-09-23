using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;
using UnityEngine;

namespace Positron.Client.NetValues.Implements
{
    public class Vector2NetValue : NetValueManagedBase<Vector2>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt32BigEndian(container, BitConverter.SingleToInt32Bits(_value.x));
            BinaryPrimitives.WriteInt32BigEndian(container.Slice(4), BitConverter.SingleToInt32Bits(_value.y));
            
            return 8;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value.x = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(container.Span));
            _value.y = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(container.Slice(4).Span));
        }
    }
}
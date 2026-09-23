using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;
using UnityEngine;

namespace Positron.Client.NetValues.Implements
{
    public sealed class Vector2IntNetValue : NetValueManagedBase<Vector2Int>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt32BigEndian(container, _value.x);
            BinaryPrimitives.WriteInt32BigEndian(container.Slice(4), _value.y);

            return 8;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value.x = BinaryPrimitives.ReadInt32BigEndian(container.Span);
            _value.y = BinaryPrimitives.ReadInt32BigEndian(container.Slice(4).Span);
        }
    }
}
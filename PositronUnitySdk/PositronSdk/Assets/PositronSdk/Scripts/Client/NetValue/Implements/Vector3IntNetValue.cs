using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;
using UnityEngine;

namespace Positron.Client.NetValues.Implements
{
    public sealed class Vector3IntNetValue : NetValueManagedBase<Vector3Int>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            BinaryPrimitives.WriteInt32BigEndian(container, _value.x);
            BinaryPrimitives.WriteInt32BigEndian(container.Slice(4), _value.y);
            BinaryPrimitives.WriteInt32BigEndian(container.Slice(8), _value.z);

            return 12;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value.x = BinaryPrimitives.ReadInt32BigEndian(container.Span);
            _value.y = BinaryPrimitives.ReadInt32BigEndian(container.Slice(4).Span);
            _value.z = BinaryPrimitives.ReadInt32BigEndian(container.Slice(8).Span);
        }
    }
}
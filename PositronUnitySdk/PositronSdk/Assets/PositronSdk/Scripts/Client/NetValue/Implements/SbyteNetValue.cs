using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues
{
    public sealed class SbyteNetValue : NetValueManagedBase<sbyte>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            container[0] = (byte)_value;
            
            return 1;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = (sbyte)container.Span[0];
        }
    }
}
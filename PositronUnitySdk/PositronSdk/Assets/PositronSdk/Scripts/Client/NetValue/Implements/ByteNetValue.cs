using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues.Implements
{
    public class ByteNetValue : NetValueManagedBase<byte>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            container[0] = _value;
            return 1;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = container.Span[0];
        }
    }
}
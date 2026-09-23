using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues.Implements
{
    public sealed class BoolNetValue : NetValueManagedBase<bool>
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer)
        {
            container[0] = (byte)(_value ? 1 : 0);

            return 1;
        }

        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = container.Span[0] == 1;
        }
    }
}
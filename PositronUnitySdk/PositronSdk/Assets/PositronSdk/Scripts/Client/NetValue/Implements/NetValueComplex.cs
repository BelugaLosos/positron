using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues.Implements
{
    public sealed class NetValueComplex<T> : NetValueManagedBase<T> where T : struct
    {
        public override int OnSerialize(Span<byte> container, IPositronSerializer serializer) => serializer.Serialize(_value, container);
        public override void OnDeserialize(ReadOnlyMemory<byte> container, IPositronSerializer serializer) => _value = serializer.Deserialize<T>(container);
    }
}
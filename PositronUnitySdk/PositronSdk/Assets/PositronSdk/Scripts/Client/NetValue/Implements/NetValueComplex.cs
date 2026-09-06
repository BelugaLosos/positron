using Positron.Client.Interfaces;
using System;

namespace Positron.Client.NetValues.Implements
{
    public class NetValueComplex<T> : INetValueManaged
    {
        private T _value;
        private uint _flatArrayDescriptor;

        public bool IsFullyInited { get; private set; }
        public T Value
        {
            get
            {
                return _value;
            }

            set
            { 
                _value = value;
                dataChangedWithFullCallback?.Invoke(this, _flatArrayDescriptor);
                changed?.Invoke();
            }
        }

        public event Action<INetValueManaged, uint> dataChangedWithFullCallback;
        public event Action changed;

        public void MarkInited(uint flatArrayIdDescriptor)
        {
            _flatArrayDescriptor = flatArrayIdDescriptor;
            IsFullyInited = true;
        }

        public int SerializeSelfTo(Span<byte> container, IPositronSerializer serializer) => serializer.Serialize(_value, container);
        public void DeserializeSelfFrom(ReadOnlyMemory<byte> container, IPositronSerializer serializer)
        {
            _value = serializer.Deserialize<T>(container);
            changed?.Invoke();
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
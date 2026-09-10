using Positron.Client.Interfaces;
using Positron.Client.Mono;
using System;
using UnityEngine;

namespace Positron.Client.NetValues.Implements
{
    public class NetValueComplex<T> : INetValueManaged
    {
        private T _value;
        private PositronNetworkIdentity _carrier;

        private uint _flatArrayDescriptor;
        private bool _predictable;

        public bool IsFullyInited { get; private set; }
        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (!_carrier.IsMine && PositronFacade.World.LocalClientId != PositronFacade.World.HostId && !_predictable)
                {
                    Debug.LogError("Attempt to change non-predictable foreign value not being a host");
                    return;
                }

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

        public void BindNetworkObject(PositronNetworkIdentity identity, bool predictable)
        {
            _carrier = identity;
            _predictable = predictable;
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
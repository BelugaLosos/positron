using Positron.Client.GameEntities;
using System;
using System.Collections.Generic;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkValuesModel : IDisposable
    {
        private readonly List<NetValue> _values = new(128);
        private readonly List<NetValue> _currentDelta = new(128);

        public void Dispose()
        {
            ClearWorld();
        }

        public void ClearWorld()
        {

        }

        public void AddOrModifyValues(NetValue[] values)
        {
            foreach (NetValue value in values)
            {
                AddOrModifyValue(value);
            }
        }

        public void AddOrModifyValue(NetValue value)
        {

        }

        public NetValue[] GetValuesDelta() => _currentDelta.ToArray();
        public void ClearDelta() => _currentDelta.Clear();
    }
}
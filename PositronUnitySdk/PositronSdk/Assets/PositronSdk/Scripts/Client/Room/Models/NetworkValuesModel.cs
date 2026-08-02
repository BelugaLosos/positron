using Positron.Client.GameEntities;
using Positron.Utility;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkValuesModel : IDisposable
    {
        private readonly PooledDynamicArraySegment<NetValue> _currentDelta = new(128);
        private byte[] _arena = new byte[16];

        public void Dispose()
        {
            ClearWorld();

            _currentDelta.Dispose();
        }

        public void ClearWorld()
        {

        }

        public void AddOrModifyValues(ArraySegment<NetValue> values, ReadOnlyMemory<byte> arena)
        {
            foreach (NetValue value in values)
            {
                AddOrModifyValue(value);
            }
        }

        public void AddOrModifyValue(NetValue value)
        {

        }

        public ArraySegment<NetValue> GetValuesDelta() => _currentDelta.ToArray();
        public ReadOnlySpan<byte> GetArena() => _arena;
        public void ClearDelta() => _currentDelta.Clear();
    }
}
using Positron.Client.GameEntities;
using Positron.Utility;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkValuesModel : IDisposable
    {
        private readonly PooledDynamicArraySegment<NetValue> _currentAddDelta = new(128);
        private readonly PooledDynamicArraySegment<PersistentNetValue> _currentModDelta = new(128);
        private byte[] _arena = new byte[16];

        public void Dispose()
        {
            ClearWorld();

            _currentAddDelta.Dispose();
            _currentModDelta.Dispose();
        }

        public void ClearWorld()
        {

        }

        public void PerformAddition(ArraySegment<NetValue> values, ReadOnlyMemory<byte> arena)
        {

        }

        public void PerformModification(ArraySegment<PersistentNetValue> values, ReadOnlyMemory<byte> arena)
        {

        }

        public ArraySegment<NetValue> GetValuesAddDelta() => _currentAddDelta.ToArray();
        public ArraySegment<PersistentNetValue> GetValuesModDelta() => _currentModDelta.ToArray();
        public ReadOnlySpan<byte> GetArena() => _arena;
       
        public void ClearDelta()
        {
            _currentAddDelta.Clear();
            _currentModDelta.Clear();
        }
    }
}
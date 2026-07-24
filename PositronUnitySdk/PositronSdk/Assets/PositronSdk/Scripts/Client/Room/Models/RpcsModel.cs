using Positron.Client.GameEntities;
using Positron.Utility;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable
    {
        private readonly PooledDynamicArraySegment<RpcCall> _currentCallBuffer = new(128);

        public void Dispose()
        {
            _currentCallBuffer.Dispose();
        }

        public void MultiCall(ArraySegment<RpcCall> calls)
        {
            foreach (RpcCall call in calls)
            {
                Call(call);
            }
        }

        public void Call(RpcCall call)
        {

        }

        public ArraySegment<RpcCall> GetCurrentDelta() => _currentCallBuffer.ToArray();
        public void ClearDelta() => _currentCallBuffer.Clear();
    }
}
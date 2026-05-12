using Positron.Client.GameEntities;
using System;
using System.Collections.Generic;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable
    {
        private readonly List<RpcCall> _currentCallBuffer = new(128);

        public void Dispose()
        {
            
        }

        public void MultiCall(RpcCall[] calls)
        {
            foreach (RpcCall call in calls)
            {
                Call(call);
            }
        }

        public void Call(RpcCall call)
        {

        }

        public RpcCall[] GetCurrentDelta() => _currentCallBuffer.ToArray();
        public void ClearDelta() => _currentCallBuffer.Clear();
    }
}
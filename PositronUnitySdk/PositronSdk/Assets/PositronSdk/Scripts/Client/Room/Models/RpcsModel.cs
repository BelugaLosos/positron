using Positron.Client.GameEntities;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable
    {
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
    }
}
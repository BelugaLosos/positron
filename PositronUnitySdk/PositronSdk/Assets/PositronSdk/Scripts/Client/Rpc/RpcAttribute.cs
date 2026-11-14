using Positron.Client.ConstantHolders;
using System;

namespace Positron.Client.Rpc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RpcAttribute : Attribute 
    {
        public RpcAttribute(RpcTargets targets, RpcSources source = RpcSources.Owner) { }
    }
}
using Positron.Client.ConstantHolders;
using UnityEngine;

namespace TeGst.F.A.V.C
{
    public partial class TestRpc : MonoBehaviour
    {
        private void Start()
        {
            SendRPC_Foo1(RpcTargets.RPC_ALL);
            SendRPC_Foo2(RpcTargets.RPC_ALL);
        }

        [Rpc]
        private void Foo1()
        {
               
        }

        [Rpc]
        private void Foo2()
        {

        }
    }
}
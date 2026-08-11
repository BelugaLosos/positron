using Positron.Client.ConstantHolders;
using Positron.Client.Rpc;
using UnityEngine;

namespace TeGst.F.A.V.C
{
    public partial class TestRpc : MonoBehaviour
    {
        private void Start()
        {
            SendRPC_Foo1(RpcTargets.RPC_ALL, "", 1, Vector3.zero);
            SendRPC_Foo2(RpcTargets.RPC_ALL);
            SendRPC_DealDamage(RpcTargets.RPC_TARGET, 1, 100f);
        }

        [Rpc]
        private void Foo1 (string s, uint targetClientId, Vector3 v)
        {
                  
        }

        [Rpc]
        private void Foo2()
        {

        }

        [Rpc]
        private void DealDamage(uint targetClientId, float damageAmount)
        {

        }
    }
}
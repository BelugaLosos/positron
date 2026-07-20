using Positron.Client.ConstantHolders;
using UnityEngine;

public partial class TestRpc : MonoBehaviour
{
    private void Start()
    {
        SendRPC_Foo1(RpcTargets.RPC_ALL);
    }

    [Rpc]
    private void Foo1()
    {
        
    }
}

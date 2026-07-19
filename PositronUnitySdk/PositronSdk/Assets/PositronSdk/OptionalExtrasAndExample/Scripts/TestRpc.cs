using Positron.Client.ConstantHolders;
using UnityEngine;

public partial class TestRpc : MonoBehaviour
{
    private void Start()
    {
        SendRPC_Foo(RpcTargets.RPC_ALL);
    }

    [Rpc]
    private void Foo()
    {
        
    }
}

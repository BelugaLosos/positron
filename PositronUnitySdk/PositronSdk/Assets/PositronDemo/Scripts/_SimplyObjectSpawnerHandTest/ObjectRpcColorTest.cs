using Positron.Client.ConstantHolders;
using Positron.Client.Mono;
using Positron.Client.Mono.Interfaces;
using Positron.Client.Rpc;
using UnityEngine;

public partial class ObjectRpcColorTest : MonoBehaviour, INetworkAwakeble, INetworkDestructable
{
    [SerializeField] private MeshRenderer _renderer;

    public void OnNetworkAwake()
    {
        Debug.Log("color woken");
    }

    public void OnNetworkDestroy()
    {
        Debug.Log("color down");
    }

    public void CallColorTransfer()
    {
        SendRPC_ChangeColorRpc(RpcTargets.RPC_TARGET, new(GetComponent<PositronNetworkIdentity>().OwnerClientId), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    [Rpc]
    private void ChangeColorRpc(RpcPlayerRef player, float r, float g, float b)
    {
        _renderer.material.color = new(r, g, b);
    }
}

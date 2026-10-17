using Positron.Client.ConstantHolders;
using Positron.Client.Mono;
using Positron.Client.Mono.Interfaces;
using Positron.Client.Rpc;
using TMPro;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public partial class ObjectRpcTest : MonoBehaviour, INetworkAwakeble, INetworkDestructable
    {
        [SerializeField] private TextMeshProUGUI _txt;
        
        private int _num;

        public void OnNetworkAwake()
        {
            Debug.Log("text woken");
        }

        public void OnNetworkDestroy()
        {
            Debug.Log("text down");
        }

        public void Test()
        {
            SendRPC_Foo1(RpcTargets.RPC_TARGET, new(GetComponent<PositronNetworkIdentity>().OwnerClientId), "T");
            SendRPC_Bar(RpcTargets.RPC_ALL, "A");
            SendRPC_Bar(RpcTargets.RPC_OTHERS, "O");
            SendRPC_Bar(RpcTargets.RPC_ALL_CACHED, "AC");
            SendRPC_Bar(RpcTargets.RPC_OTHERS_CACHED, "OC");
            SendRPC_Bar(RpcTargets.RPC_HOST_COMMAND, "H");
        }

        [Rpc]
        private void Foo1(RpcPlayerRef palyer, string s)
        {
            _num++;
            _txt.text += "1" + s + _num.ToString() + "; ";
        }

        [Rpc]
        private void Bar(string d)
        {
            _txt.text += d + " ";
        }
    }
}
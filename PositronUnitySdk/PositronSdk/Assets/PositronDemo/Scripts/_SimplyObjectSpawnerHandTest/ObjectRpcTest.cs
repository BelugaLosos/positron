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
            SendRPC_Foo1(RpcTargets.RPC_TARGET, new(GetComponent<PositronNetworkIdentity>().OwnerClientId), "TRG");
            SendRPC_Bar(RpcTargets.RPC_ALL, "ALL");
            SendRPC_Bar(RpcTargets.RPC_OTHERS, "OTHERS");
            SendRPC_Bar(RpcTargets.RPC_ALL_CACHED, "A CACHED");
            SendRPC_Bar(RpcTargets.RPC_OTHERS_CACHED, "O CACHED");
            SendRPC_Bar(RpcTargets.RPC_HOST_COMMAND, "HOST CMD");
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
            _txt.text += " " + d;
        }
    }
}
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
            SendRPC_BarAwaken1(RpcTargets.RPC_ALL, Random.Range(1, 100));
            Invoke(nameof(Dellayed), 1f/33f + Time.fixedDeltaTime);
            Debug.Log("text woken");
        }

        private void Dellayed()
        {
            SendRPC_BarAwaken2(RpcTargets.RPC_ALL, Random.Range(1, 100));
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

        [Rpc]
        private void BarAwaken1(int rv)
        {
            _txt.text += " BA1 " + rv.ToString();
        }

        [Rpc]
        private void BarAwaken2(int rv)
        {
            _txt.text += " BA2 " + rv.ToString();
        }
    }
}
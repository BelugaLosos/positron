using Positron.Client.ConstantHolders;
using Positron.Client.Mono;
using Positron.Client.Mono.Interfaces;
using Positron.Client.Rpc;
using TMPro;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public sealed partial class ObjectRpcTest : MonoBehaviour, INetworkAwakeble, INetworkDestructable
    {
        [SerializeField] private TextMeshProUGUI _txt;
        
        private int _num;

        public void OnNetworkAwake()
        {
            SendRPC_BarAwaken1(Random.Range(1, 100));
            Invoke(nameof(Dellayed), 1f/33f + Time.fixedDeltaTime);
            Debug.Log("text woken");
        }

        private void Dellayed()
        {
            SendRPC_BarAwaken2(Random.Range(1, 100));
        }

        public void OnNetworkDestroy()
        {
            Debug.Log("text down");
        }

        public void Test()
        {
            SendRPC_Foo1(new(GetComponent<PositronNetworkIdentity>().OwnerClientId), "T");
            SendRPC_BarAll("A");
            SendRPC_BarOthers("O");
            SendRPC_BarAllCached("AC");
            SendRPC_BarOthersCached("OC");
            SendRPC_BarHostCommand("H");
        }

        [Rpc(RpcTargets.RPC_TARGET, RpcSources.Host)]
        private void Foo1(RpcPlayerRef player, string s)
        {
            _num++;
            _txt.text += "1" + s + _num.ToString() + "; ";
        }



        [Rpc(RpcTargets.RPC_ALL)]
        private void BarAll(string d)
        {
            _txt.text += d + " ";
        }

        [Rpc(RpcTargets.RPC_OTHERS)]
        private void BarOthers(string d)
        {
            _txt.text += d + " ";
        }

        [Rpc(RpcTargets.RPC_ALL_CACHED)]
        private void BarAllCached(string d)
        {
            _txt.text += d + " ";
        }

        [Rpc(RpcTargets.RPC_OTHERS_CACHED)]
        private void BarOthersCached(string d)
        {
            _txt.text += d + " ";
        }

        [Rpc(RpcTargets.RPC_HOST_COMMAND)]
        private void BarHostCommand(string d)
        {
            _txt.text += d + " ";
        }



        [Rpc(RpcTargets.RPC_ALL)]
        private void BarAwaken1(int rv)
        {
            _txt.text += " BA1 " + rv.ToString();
        }

        [Rpc(RpcTargets.RPC_ALL)]
        private void BarAwaken2(int rv)
        {
            _txt.text += " BA2 " + rv.ToString();
        }
    }
}
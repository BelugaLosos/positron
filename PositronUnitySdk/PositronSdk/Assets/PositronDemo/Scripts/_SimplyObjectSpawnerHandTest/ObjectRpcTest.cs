using Positron.Client.ConstantHolders;
using Positron.Client.Rpc;
using TMPro;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public partial class ObjectRpcTest : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _txt;
        
        private int _num;
        
        public void Test()
        {
            for (int i = 0; i < 50; i++)
            {
                SendRPC_Foo1(RpcTargets.RPC_ALL_CACHED, "ss");
            }
        }

        [Rpc]
        private void Foo1(string s)
        {
            _num++;
            _txt.text += "1" + s + _num.ToString() + "; ";
        }
    }
}
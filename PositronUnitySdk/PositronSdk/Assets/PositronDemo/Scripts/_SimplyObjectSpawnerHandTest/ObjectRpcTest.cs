using Positron.Client.ConstantHolders;
using Positron.Client.Rpc;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public partial class ObjectRpcTest : MonoBehaviour
    {
        private int _next;

        public void GenerateRandomScale()
        {
            _next++;

            _next = Mathf.Clamp(_next, 1, 2);
            
            SendRPC_SetScale(RpcTargets.RPC_ALL_CACHED, _next);
            
            if (_next == 2)
            {
                _next = 0;
            }

            SendRPC_SetScaleFloat(RpcTargets.RPC_ALL_CACHED, Random.Range(1f, 2f));
        }

        [Rpc]
        private void SetScale(int scaleMod)
        {
            transform.localScale = Vector3.one * scaleMod;
            Debug.Log($"I {scaleMod}");
        }

        [Rpc]
        private void SetScaleFloat(float scaleF)
        {
            transform.localScale = Vector3.one * scaleF;
            Debug.Log($"F {scaleF}");
        }
    }
}
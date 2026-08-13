using Positron.Client.ConstantHolders;
using Positron.Client.Mono;
using Positron.Client.Rpc;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public partial class ObjectRpcTest : MonoBehaviour
    {
        private float _next = 1;

        private void Awake()
        {
            GenerateRandomScale();
        }

        public void GenerateRandomScale()
        {
            _next++;

            _next = Mathf.Clamp(_next, 1, 1.6434458f);
            
            if (_next > 1.4f)
            {
                _next = 1.28f;
            }

            SendRPC_SetScaleFloat(RpcTargets.RPC_ALL_CACHED, _next);
        }

        [Rpc]
        private void SetScaleFloat(float scaleF)
        {
            transform.localScale = Vector3.one * scaleF;
            Debug.Log($"F {scaleF}");
        }
    }
}
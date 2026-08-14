using Positron.Client.ConstantHolders;
using Positron.Client.Mono.Interfaces;
using Positron.Client.Rpc;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public partial class ObjectRpcTest : MonoBehaviour, INetworkAwakeble
    {
        private float _next = 1;

        public void OnNetworkAwake()
        {
            GenerateRandomScale();
        }

        public void GenerateRandomScale()
        {
            _next++;

            _next = Mathf.Clamp(_next, 1, 1.6434458f);

            SendRPC_SetScaleFloat(RpcTargets.RPC_ALL_CACHED, _next);

            if (_next > 1.4f)
            {
                _next = 0f;
            }
        }

        [Rpc]
        private void SetScaleFloat(float scaleF)
        {
            transform.localScale = Vector3.one * scaleF;
            Debug.Log($"F {scaleF}");
        }
    }
}
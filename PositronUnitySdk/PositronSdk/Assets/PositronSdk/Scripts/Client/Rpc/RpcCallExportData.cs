using Positron.Client.ConstantHolders;
using System;
using UnityEngine;

namespace Positron.Client.Rpc
{
    [Serializable]
    public struct RpcCallExportData
    {
        [SerializeField] private string _rpcSourceName;
        [SerializeField] private ulong _rpcName;
        [SerializeField] private RpcTargets _rpcTarget;
        [SerializeField] private RpcSources _rpcSource;

        public string RpcSourceName => _rpcSourceName;
        public ulong RpcName => _rpcName;
        public RpcTargets RpcTarget => _rpcTarget;
        public RpcSources RpcSource => _rpcSource;

        public RpcCallExportData(string rpcSourceName, ulong rpcName, RpcTargets rpcTarget, RpcSources rpcSource)
        {
            _rpcSourceName = rpcSourceName;
            _rpcName = rpcName;
            _rpcTarget = rpcTarget;
            _rpcSource = rpcSource;
        }
    }
}
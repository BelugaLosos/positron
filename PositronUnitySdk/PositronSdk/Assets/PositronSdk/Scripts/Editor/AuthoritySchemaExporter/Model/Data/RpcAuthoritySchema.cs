using Positron.Client.ConstantHolders;
using System;
using UnityEngine;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    /// <summary>
    /// id of rpc here stripped out, id of this data in array IS rpc global id
    /// </summary>
    [Serializable]
    public struct RpcAuthoritySchema
    {
        [SerializeField] private bool _isUsed;
        [SerializeField] private RpcTargets _rpcTarget;
        [SerializeField] private RpcSources _rpcSource;
    
        public RpcAuthoritySchema(bool isUsed, RpcTargets rpcTarget, RpcSources rpcSource)
        {
            _isUsed = isUsed;
            _rpcTarget = rpcTarget;
            _rpcSource = rpcSource;
        }
    }
}
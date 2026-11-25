using UnityEngine;
using System;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct AuthoritySchemaDto
    {
        [SerializeField] private PrefabAuthoritySchema[] _prefabsAuthoritySchemas;
        [SerializeField] private NetValueAuthorityObjectContainerSchema[] _netValuesAuthoritySchemas;
        [SerializeField] private RpcAuthoritySchema[] _rpcAuthoritySchema;

        public AuthoritySchemaDto(
            PrefabAuthoritySchema[] prefabsAuthoritySchemas, 
            NetValueAuthorityObjectContainerSchema[] netValuesAuthoritySchemas,
            RpcAuthoritySchema[] rcpAuthoritySchema)
        {
            _prefabsAuthoritySchemas = prefabsAuthoritySchemas;
            _netValuesAuthoritySchemas = netValuesAuthoritySchemas;
            _rpcAuthoritySchema = rcpAuthoritySchema;
        }
    }
}
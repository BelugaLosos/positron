using UnityEngine;
using System;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct AuthoritySchemaDto
    {
        [SerializeField] private PrefabAuthoritySchema[] _prefabsAuthoritySchemas;
        [SerializeField] private NetValueAuthorityObjectContainerSchema[] _netValuesAuthoritySchemas;

        public AuthoritySchemaDto(PrefabAuthoritySchema[] prefabsAuthoritySchemas, NetValueAuthorityObjectContainerSchema[] netValuesAuthoritySchemas)
        {
            _prefabsAuthoritySchemas = prefabsAuthoritySchemas;
            _netValuesAuthoritySchemas = netValuesAuthoritySchemas;
        }
    }
}
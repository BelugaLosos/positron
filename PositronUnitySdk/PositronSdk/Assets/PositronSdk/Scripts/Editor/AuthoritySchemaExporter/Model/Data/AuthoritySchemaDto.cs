using UnityEngine;
using System;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct AuthoritySchemaDto
    {
        [SerializeField] private PrefabAuthoritySchema[] _prefabsAuthoritySchemas;
        [SerializeField] private NetValueAuthoritySchema[] _netValuesAuthoritySchemas;

        public AuthoritySchemaDto(PrefabAuthoritySchema[] prefabsAuthoritySchemas, NetValueAuthoritySchema[] netValuesAuthoritySchemas)
        {
            _prefabsAuthoritySchemas = prefabsAuthoritySchemas;
            _netValuesAuthoritySchemas = netValuesAuthoritySchemas;
        }
    }
}
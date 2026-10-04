using UnityEngine;
using System;
using Positron.Client.NetValues;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct NetValueAuthoritySchema
    {
        [SerializeField] private NetValueAuthority _auhority;

        public NetValueAuthoritySchema( NetValueAuthority nva)
        {
            _auhority = nva;
        }
    }
}
using UnityEngine;
using System;
using Positron.Client.NetValues;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct NetValueAuthoritySchema
    {
        [SerializeField] private ushort _prefabId;
        [SerializeField] private ushort _valueIdOnObject;
        [SerializeField] private NetValueAuthority _auhority;

        public NetValueAuthoritySchema(ushort pid, ushort vidoo, NetValueAuthority nva)
        {
            _prefabId = pid;
            _valueIdOnObject = vidoo;
            _auhority = nva;
        }
    }
}
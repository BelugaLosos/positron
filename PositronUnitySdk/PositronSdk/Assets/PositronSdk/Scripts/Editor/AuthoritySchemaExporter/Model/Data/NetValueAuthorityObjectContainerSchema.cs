using UnityEngine;
using System;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct NetValueAuthorityObjectContainerSchema
    {
        [SerializeField] private NetValueAuthoritySchema[] _netValuesOfObject;

        public NetValueAuthorityObjectContainerSchema(NetValueAuthoritySchema[] aus)
        {
            _netValuesOfObject = aus;
        }
    }
}
using UnityEngine;
using Positron.Client.NetworkPrefabsAuthorities;
using System;

namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    [Serializable]
    public struct PrefabAuthoritySchema
    {
        [SerializeField] private ushort _id;
        [SerializeField] private PrefabSpawnAuthority _spawnAuthority;
        [SerializeField] private PrefabDestroyAuthority _destroyAuthority;

        public PrefabAuthoritySchema(ushort id, PrefabSpawnAuthority sa, PrefabDestroyAuthority da)
        {
            _id = id;
            _spawnAuthority = sa;
            _destroyAuthority = da;
        }
    }
}
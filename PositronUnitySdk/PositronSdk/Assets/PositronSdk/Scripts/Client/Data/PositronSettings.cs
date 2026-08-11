using Positron.Client.Mono;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;
using System.Buffers.Binary;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System;
#endif

namespace Positron.Client.Settings
{
    [CreateAssetMenu(fileName = "PositronSettings", menuName = "Positron/NetworkSettings")]
    public sealed class PositronSettings : ScriptableObject
    {
        [field: SerializeField] public string Address { get; private set; }
        [field: SerializeField] public int Port { get; private set; }
        [field: SerializeField] public bool IsSecure { get; private set; }
        [field: SerializeField] public bool Autoconnect { get; private set; }
        [SerializeField][Min(1)] private int _tickrate = 30;
        [field: SerializeField] public string Version { get; private set; } = "0.0.1 -- DEFAULT";
        [field: SerializeField] public double TickOffset { get; private set; } = 2d;
        [field: SerializeField] public bool UseTransformsResync = true;
        [field: SerializeField] public uint TicksAmountToMarkObjectAsStatic = 150;
        [field: SerializeField] public uint MaximalObjectsCountForRetransmitPerPacket = 50;
        [field: SerializeField] public PositronNetworkIdentity[] SpawnableObjects { get; private set; }
        [field: SerializeField] public ulong[] RpcMethodsNames { get; private set; }

        public int Tickrate => _tickrate;

        public static readonly string RESOURCES_PATH = "PositronSettings";

        private void OnValidate()
        {
            if (SpawnableObjects.Length > ushort.MaxValue)
            {
                SpawnableObjects = SpawnableObjects.Take(ushort.MaxValue).ToArray();
                Debug.LogError("Too many objects to spawn");
            }
        }

#if UNITY_EDITOR
        public void SetRpcMapping(string[] names)
        {
            var hashes = new ulong[names.Length];
            var hashToName = new Dictionary<ulong, string>(names.Length);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];

                byte[] nameBytes = Encoding.UTF8.GetBytes(name);

                using SHA256 hasher = SHA256.Create();
                byte[] digest = hasher.ComputeHash(nameBytes);
                ulong hash = BinaryPrimitives.ReadUInt64BigEndian(digest.AsSpan(0, 8));

                if (hashToName.TryGetValue(hash, out string existingName))
                {
                    RpcMethodsNames = null;

                    Debug.LogError(
                        $"RPC HASH COLLISION!\n" +
                        $"Hash: 0x{hash:X16}\n" +
                        $"RPC 1: {existingName}\n" +
                        $"RPC 2: {name}\n" +
                        $"Change the name of one of these RPCs to prevent the collision.");

                    EditorUtility.SetDirty(this);
                    return;
                }

                hashToName.Add(hash, name);
                hashes[i] = hash;
            }

            RpcMethodsNames = hashes;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
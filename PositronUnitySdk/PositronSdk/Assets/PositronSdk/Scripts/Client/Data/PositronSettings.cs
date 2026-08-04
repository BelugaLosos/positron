using Positron.Client.Mono;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
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
        [field: SerializeField] public string[] RpcMethodsNames { get; private set; }

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
            RpcMethodsNames = names;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
using UnityEngine;

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

        public int Tickrate => _tickrate;
    }
}
using UnityEngine;

namespace Positron.Client.Settings
{
    [CreateAssetMenu(fileName = "PositronSettings", menuName = "Positron/NetworkSettings")]
    public sealed class PositronSettings : ScriptableObject
    {
        [field: SerializeField] public string Address { get; private set; }
        [field: SerializeField] public int Port { get; private set; }
        [field: SerializeField] public bool IsSecure { get; private set; }
    }
}
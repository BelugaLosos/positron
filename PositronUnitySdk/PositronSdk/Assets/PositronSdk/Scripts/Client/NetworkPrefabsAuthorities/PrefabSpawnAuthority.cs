namespace Positron.Client.NetworkPrefabsAuthorities
{
    public enum PrefabSpawnAuthority : byte
    {
        /// <summary>
        /// prefab can be spawned by anybody
        /// </summary>
        Any = 0x0,
        /// <summary>
        /// only host can spawn prefab
        /// </summary>
        Host = 0x1
    }
}
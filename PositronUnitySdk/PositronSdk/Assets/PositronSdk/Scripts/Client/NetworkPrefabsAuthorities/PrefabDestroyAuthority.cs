namespace Positron.Client.NetworkPrefabsAuthorities
{
    public enum PrefabDestroyAuthority : byte
    {
        /// <summary>
        /// anybody can destroy object of this prefab
        /// </summary>
        Any = 0x0,
        /// <summary>
        /// owner and host can destroy object of this prefab
        /// </summary>
        Owner = 0x1,
        /// <summary>
        /// only host can destroy object of this prefab
        /// </summary>
        Host = 0x2
    }
}
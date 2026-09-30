namespace Positron.Editor.AuthoritySchemaExporter.Data
{
    public enum PrefabOwnershipAuthority : byte
    {
        /// <summary>
        /// nobody can transfer ownership of this object of this prefab excluding disconnect of owner and autonatic transfer to a host
        /// </summary>
        Forbidden = 0x0,
        /// <summary>
        /// only host can transfer ownership of this object of this prefab
        /// </summary>
        HostOnly = 0x1,
        /// <summary>
        /// anybody can transfer object of this prefab
        /// </summary>
        Any = 0x2 
    }
}
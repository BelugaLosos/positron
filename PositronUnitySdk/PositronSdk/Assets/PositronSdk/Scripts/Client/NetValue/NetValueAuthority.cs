namespace Positron.Client.NetValues
{
    public enum NetValueAuthority : byte
    {
        /// <summary>
        /// only owner client can change value
        /// </summary>
        Owner = 0x0,
        /// <summary>
        /// only server can change value
        /// </summary>
        Host = 0x1,
        /// <summary>
        /// anyone can change it
        /// </summary>
        Any = 0x2
    }
}
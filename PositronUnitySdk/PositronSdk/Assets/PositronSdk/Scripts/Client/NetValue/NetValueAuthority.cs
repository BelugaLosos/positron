namespace Positron.Client.NetValues
{
    public enum NetValueAuthority : byte
    {
        /// <summary>
        /// owner client and server can change value
        /// </summary>
        Owner = 0x0,
        /// <summary>
        /// only server can change value
        /// </summary>
        Server = 0x1
    }
}
namespace Positron.Client.NetValues
{
    public enum NetValueAuthority : byte
    {
        /// <summary>
        /// owner client and server can change value
        /// </summary>
        Owner,
        /// <summary>
        /// only server can change value
        /// </summary>
        Server
    }
}
namespace Positron.Client.Rpc
{
    public ref struct RpcPlayerRef
    {
        public uint TargetClientId { get; set; }

        public RpcPlayerRef(uint targetClientId)
        {
            TargetClientId = targetClientId;
        }
    }
}
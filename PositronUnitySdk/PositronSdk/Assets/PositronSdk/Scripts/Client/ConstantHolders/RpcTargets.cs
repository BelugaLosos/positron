namespace Positron.Client.ConstantHolders
{
    public enum RpcTargets : byte
    {
        RPC_ALL = 0x0,
        RPC_OTHERS = 0x1,
        RPC_TARGET = 0x2,
        RPC_ALL_CACHED = 0x3,
        RPC_OTHERS_CACHED = 0x4,
        RPC_TARGET_CACHED = 0x5
    }
}
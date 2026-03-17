using MessagePack;

namespace Positron.Client.GameEntities
{
    [MessagePackObject]
    public struct RpcCall
    {
        [Key(0)] public uint ObjectId { get; set; }
        [Key(1)] public uint TargetClientId { get; set; }
        [Key(2)] public ushort SubObjectId { get; set; }
        [Key(3)] public byte Type { get; set; }
        [Key(4)] public string MethodName { get; set; }
        [Key(5)] public byte[] Args { get; set; }
    }
}
using MessagePack;

namespace Positron.Client.GameEntities
{
    [MessagePackObject]
    public struct NetValue
    {
        [Key(0)] public ulong CreationId { get; set; }
        [Key(1)] public uint ParentObjectId { get; set; }
        [Key(2)] public ushort SubObjectId { get; set; }
        [Key(3)] public bool Deleting { get; set; }
        [Key(4)] public byte[] Payload { get; set; }
    }
}
using MessagePack;

namespace Positron.Client.GameEntities
{
    [MessagePackObject]
    public struct NetValue
    {
        [Key(0)] public uint ArenaPtr { get; set; }
        [Key(1)] public uint ArenaLen { get; set; }
        [Key(2)] public uint ParentObjectId { get; set; }
        [Key(3)] public uint FlatArrayIdDescriptor { get; set; }
        [Key(4)] public ushort ValueId { get; set; }
        [Key(5)] public bool Deleting { get; set; }
    }
}
using MessagePack;

namespace Positron.Client.GameEntities
{
    [MessagePackObject]
    public struct PersistentNetValue
    {
        [Key(0)] public uint FlatArrayIdDescriptor { get; set; }
        [Key(1)] public uint ArenaPtr { get; set; }
        [Key(2)] public uint ArenaLen { get; set; }
        [Key(3)] public bool IsDeleting { get; set; }
    }
}
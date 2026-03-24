using MessagePack;
using Positron.Client.GameEntities;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct GameUnreliableTick
    {
        [Key(0)] public ulong TimeStamp { get; set; }
        [Key(1)] public uint ClientId { get; set; }
        [Key(2)] public NetTransform[] MovedObjects { get; set; }
    }
}
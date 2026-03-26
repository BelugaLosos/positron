using MessagePack;
using Positron.Client.GameEntities;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct GameUnreliableTick
    {
        [Key(0)] public uint ClientId { get; set; }
        [Key(1)] public NetTransform[] MovedObjects { get; set; }
    }
}
using MessagePack;
using Positron.Client.GameEntities.Premitive;

namespace Positron.Client.GameEntities
{
    [MessagePackObject]
    public struct NetTransform
    {
        [Key(0)] public uint ObjectId { get; set; }
        [Key(1)] public NetVector3 Position { get; set; }
        [Key(2)] public NetVector3 Rotation { get; set; }
    }
}
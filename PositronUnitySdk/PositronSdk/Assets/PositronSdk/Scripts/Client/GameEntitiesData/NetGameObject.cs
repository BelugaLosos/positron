using MessagePack;
using Positron.Client.GameEntities.Premitive;

namespace Positron.Client.GameEntities
{
    [MessagePackObject]
    public struct NetGameObject
    {
        [Key(0)] public ulong AssetIndex { get; set; }
        [Key(1)] public ulong CreationId { get; set; }
        [Key(2)] public uint ObjectId { get; set; }
        [Key(3)] public uint OwnerClientId { get; set; }
        [Key(4)] public NetVector3 Position { get; set; }
        [Key(5)] public NetVector3 Rotation { get; set; }
    }
}
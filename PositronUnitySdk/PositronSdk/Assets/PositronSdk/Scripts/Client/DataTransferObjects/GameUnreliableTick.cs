using MessagePack;
using Positron.Client.GameEntities;
using System;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct GameUnreliableTick
    {
        [Key(0)] public uint Tick { get; set; }
        [Key(1)] public uint ClientId { get; set; }
        [Key(2)] public ArraySegment<NetTransform> MovedObjects { get; set; }
    }
}
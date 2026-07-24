using MessagePack;
using Positron.Client.GameEntities;
using System;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct GameTickPacket
    {
        [Key(0)] public uint Tick { get; set; }
        [Key(1)] public uint Host { get; set; }
        [Key(2)] public uint Client { get; set; }
        [Key(3)] public ArraySegment<NetGameObject> NewGameObjects { get; set; }
        [Key(4)] public ArraySegment<uint> RemovedObjects { get; set; }
        [Key(5)] public ArraySegment<uint> TransferedObjects { get; set; }
        [Key(6)] public ArraySegment<NetValue> ValueModification { get; set; }
        [Key(7)] public ArraySegment<RpcCall> Rpcs { get; set; }
    }
}
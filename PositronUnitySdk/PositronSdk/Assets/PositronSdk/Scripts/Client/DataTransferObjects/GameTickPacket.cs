using MessagePack;
using Positron.Client.GameEntities;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct GameTickPacket
    {
        [Key(0)] public uint Tick { get; set; }
        [Key(1)] public uint Host { get; set; }
        [Key(2)] public uint Client { get; set; }
        [Key(3)] public NetGameObject[] NewGameObjects { get; set; }
        [Key(4)] public uint[] RemovedObjects { get; set; }
        [Key(5)] public uint[] TransferedObjects { get; set; }
        [Key(6)] public NetValue[] ValueModification { get; set; }
        [Key(7)] public RpcCall[] Rpcs { get; set; }
    }
}
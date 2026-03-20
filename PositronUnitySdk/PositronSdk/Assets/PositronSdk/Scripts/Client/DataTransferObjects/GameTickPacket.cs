using MessagePack;
using Positron.Client.GameEntities;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct GameTickPacket
    {
        [Key(0)] public uint Host { get; set; }
        [Key(1)] public uint Client { get; set; }
        [Key(2)] public NetGameObject[] NewGameObjects { get; set; }
        [Key(3)] public uint[] RemovedObjects { get; set; }
        [Key(4)] public uint[] TransferedToHostObjects { get; set; }
        [Key(5)] public NetValue[] ValueModification { get; set; }
        [Key(6)] public RpcCall[] Rpcs { get; set; }
    }
}
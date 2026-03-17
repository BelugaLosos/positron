using MessagePack;
using Positron.Client.GameEntities;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct JoinRoomResponse
    {
        [Key(0)] public NetGameObject[] GameObjects { get; set; }
        [Key(1)] public NetValue[] Values { get; set; }
        [Key(2)] public RpcCall[] CachedRpcCalls { get; set; }
        [Key(3)] public uint SelfId { get; set; }
        [Key(4)] public uint Host { get; set; }
    }
}
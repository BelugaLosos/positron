using MessagePack;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct JoinRoomRequest
    {
        [Key(0)] public string Uuid { get; set; }
    }
}
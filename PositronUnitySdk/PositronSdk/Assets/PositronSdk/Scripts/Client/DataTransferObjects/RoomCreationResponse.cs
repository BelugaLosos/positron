using MessagePack;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct RoomCreationResponse
    {
        [Key(0)] public string Uuid { get; set; }
    }
}
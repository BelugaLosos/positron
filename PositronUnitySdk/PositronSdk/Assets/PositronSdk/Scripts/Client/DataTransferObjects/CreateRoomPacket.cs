using MessagePack;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct CreateRoomPacket
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public int PlayerCap { get; set; }
    }
}
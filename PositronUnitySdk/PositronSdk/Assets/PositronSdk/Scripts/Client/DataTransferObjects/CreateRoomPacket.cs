using MessagePack;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct CreateRoomPacket
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public uint PlayerCap { get; set; }
        [Key(2)] public uint Scene { get; set; }
        [Key(3)] public byte[] ExternalData { get; set; }
    }
}
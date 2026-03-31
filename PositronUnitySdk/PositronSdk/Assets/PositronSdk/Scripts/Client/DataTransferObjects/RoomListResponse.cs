using MessagePack;

namespace Positron.Client.DataTransferObjects
{
    [MessagePackObject]
    public struct RoomListResponse
    {
        [Key(0)] public RoomsListElement[] List { get; set; }
    }

    [MessagePackObject]
    public struct RoomsListElement
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public string Uuid { get; set; }
        [Key(2)] public uint CurrentPlayers { get; set; }
        [Key(3)] public uint MaxPlayers { get; set; }
        [Key(4)] public uint Scene { get; set; }
        [Key(5)] public byte[] ExternalData { get; set; }
    }
}
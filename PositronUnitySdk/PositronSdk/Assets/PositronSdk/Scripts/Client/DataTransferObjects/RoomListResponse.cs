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
        [Key(2)] public int CurrentPlayers { get; set; }
        [Key(3)] public int MaxPlayers { get; set; }
    }
}
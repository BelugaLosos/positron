package datatransferobjects

type RoomCreationResponsePacket struct {
	uuid string
}

func NewRoomCreationResponsePacket(uuid string) *RoomCreationResponsePacket {
	return &RoomCreationResponsePacket{
		uuid: uuid,
	}
}

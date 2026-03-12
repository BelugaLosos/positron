package datatransferobjects

type RoomCreationResponsePacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	uuid string
}

func NewRoomCreationResponsePacket(uuid string) *RoomCreationResponsePacket {
	return &RoomCreationResponsePacket{
		uuid: uuid,
	}
}

package datatransferobjects

type RoomCreationResponsePacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	Uuid string
}

func NewRoomCreationResponsePacket(uuid string) *RoomCreationResponsePacket {
	return &RoomCreationResponsePacket{
		Uuid: uuid,
	}
}

package datatransferobjects

type CreateRoomPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	Name         string
	PlayerCap    uint32
	Scene        uint32
	Tickrate     uint32
	ExternalData []byte
}

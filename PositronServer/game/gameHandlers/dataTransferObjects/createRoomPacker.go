package datatransferobjects

type CreateRoomPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	name      string
	playerCap int32
}

func (c *CreateRoomPacket) GetName() string {
	return c.name
}

func (c *CreateRoomPacket) GetPlayerCap() int32 {
	if c.playerCap <= 0 {
		return 1
	}

	return c.playerCap
}

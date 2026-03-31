package datatransferobjects

type CreateRoomPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	name         string
	playerCap    uint32
	scene        uint32
	externalData []byte
}

func (c *CreateRoomPacket) GetName() string {
	return c.name
}

func (c *CreateRoomPacket) GetPlayerCap() uint32 {
	if c.playerCap <= 0 {
		return 1
	}

	return c.playerCap
}

func (c *CreateRoomPacket) GetScene() uint32 {
	return c.scene
}

func (c *CreateRoomPacket) GetExternalData() []byte {
	return c.externalData
}

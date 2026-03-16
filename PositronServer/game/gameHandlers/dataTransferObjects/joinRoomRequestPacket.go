package datatransferobjects

type JoinRoomRequestPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	uuid string
}

func (j *JoinRoomRequestPacket) GetTargetUuid() string {
	return j.uuid
}

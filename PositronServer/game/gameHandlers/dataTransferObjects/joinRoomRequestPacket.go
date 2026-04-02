package datatransferobjects

type JoinRoomRequestPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	Uuid string
}

func (j *JoinRoomRequestPacket) GetTargetUuid() string {
	return j.Uuid
}

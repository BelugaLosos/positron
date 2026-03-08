package gameentities

type RpcCall struct {
	objectId     uint32
	targetClient uint32
	subObjectId  uint16
	targets      uint8
	methodName   string
	args         []byte
}

func (r *RpcCall) GetObjectId() uint32 {
	return r.objectId
}

func (r *RpcCall) GetTargetClient() uint32 {
	return r.targetClient
}

func (r *RpcCall) GetSubObjectId() uint16 {
	return r.subObjectId
}

func (r *RpcCall) GetTarget() uint8 {
	return r.targets
}

func (r *RpcCall) GetMethodName() string {
	return r.methodName
}

func (r *RpcCall) GetArgs() []byte {
	return r.args
}

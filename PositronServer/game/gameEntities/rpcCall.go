package gameentities

type RpcCall struct {
	_msgpack struct{} `msgpack:",as_array"`

	ObjectId     uint32
	TargetClient uint32
	SubObjectId  uint16
	RpcType      uint8
	MethodName   string
	Args         []byte
}

func NewRpcCall(objId uint32, targetClient uint32, subObjectsId uint16, rpcType uint8, methodName string, agrs []byte) *RpcCall {
	return &RpcCall{
		ObjectId:     objId,
		TargetClient: targetClient,
		SubObjectId:  subObjectsId,
		RpcType:      rpcType,
		MethodName:   methodName,
		Args:         agrs,
	}
}

func (r *RpcCall) GetObjectId() uint32 {
	return r.ObjectId
}

func (r *RpcCall) GetTargetClient() uint32 {
	return r.TargetClient
}

func (r *RpcCall) GetSubObjectId() uint16 {
	return r.SubObjectId
}

func (r *RpcCall) GetTarget() uint8 {
	return r.RpcType
}

func (r *RpcCall) GetMethodName() string {
	return r.MethodName
}

func (r *RpcCall) GetArgs() []byte {
	return r.Args
}

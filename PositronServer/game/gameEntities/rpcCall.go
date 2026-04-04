package gameentities

import "github.com/vmihailenco/msgpack/v5"

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

func (r *RpcCall) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.EncodeArrayLen(6)
	err := enc.EncodeUint32(r.ObjectId)

	if err != nil {
		return err
	}

	err = enc.EncodeUint32(r.TargetClient)

	if err != nil {
		return err
	}

	err = enc.EncodeUint16(r.SubObjectId)

	if err != nil {
		return err
	}

	err = enc.EncodeUint8(r.RpcType)

	if err != nil {
		return err
	}

	err = enc.EncodeString(r.MethodName)

	if err != nil {
		return err
	}

	enc.EncodeArrayLen(len(r.Args))
	err = enc.EncodeBytes(r.Args)

	return err
}

func (r *RpcCall) DecodeMsgpack(dec *msgpack.Decoder) error {
	dec.DecodeArrayLen()
	id, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	clientId, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	subId, err := dec.DecodeUint16()

	if err != nil {
		return err
	}

	typeId, err := dec.DecodeUint8()

	if err != nil {
		return err
	}

	method, err := dec.DecodeString()

	if err != nil {
		return err
	}

	dec.DecodeArrayLen()
	args, err := dec.DecodeBytes()

	r.ObjectId = id
	r.TargetClient = clientId
	r.SubObjectId = subId
	r.RpcType = typeId
	r.MethodName = method
	r.Args = args

	return err
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

package gameentities

import (
	"encoding/binary"
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type RpcCall struct {
	TargetClient uint32
	ObjectId     uint32
	SubObjectId  uint16
	RpcType      uint8
	MethodName   string
	Args         []byte
}

func NewRpcCall(objId uint32, targetClient uint32, subObjectsId uint16, rpcType uint8, methodName string, agrs []byte, useRawArgs bool) *RpcCall {
	var argsBuf []byte

	if useRawArgs {
		argsBuf = agrs
	} else {
		argsBuf = make([]byte, len(agrs)+1)
		argsBuf[0] = 0
		copy(argsBuf[1:], agrs)
	}

	return &RpcCall{
		ObjectId:     objId,
		TargetClient: targetClient,
		SubObjectId:  subObjectsId,
		RpcType:      rpcType,
		MethodName:   methodName,
		Args:         argsBuf,
	}
}

func (r *RpcCall) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(6)
	err := enc.EncodeUint(uint64(r.ObjectId))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.TargetClient))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.SubObjectId))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.RpcType))

	if err != nil {
		return err
	}

	err = enc.EncodeString(r.MethodName)

	if err != nil {
		return err
	}

	err = enc.EncodeBytes(r.Args)

	return err
}

func (r *RpcCall) DecodeMsgpack(dec *msgpack.Decoder) error {
	arrLen, arrErr := dec.DecodeArrayLen()
	id, err := dec.DecodeUint32()

	if arrErr != nil {
		return arrErr
	}

	if arrLen != 6 {
		return errors.New("Rpc call arr invalid")
	}

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
	if r.Args[0] == 1 {
		return r.Args[5:]
	}

	return r.Args[1:]
}

func (r *RpcCall) TryGetCreationId() (bool, uint32) {
	if r.Args[0] == 1 {
		encoded := r.Args[1:5]
		return true, binary.BigEndian.Uint32(encoded)
	}

	return false, 0
}

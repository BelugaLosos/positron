package gameentities

import (
	"encoding/binary"
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type RpcCall struct {
	args         []byte
	targetClient uint32
	objectId     uint32
	methodId     uint16
	subObjectId  uint16
	rpcType      uint8
}

func NewRpcCall(objId uint32, targetClient uint32, subObjectsId uint16, rpcType uint8, methodId uint16, agrs []byte, useRawArgs bool) RpcCall {
	var argsBuf []byte

	if useRawArgs {
		argsBuf = agrs
	} else {
		argsBuf = make([]byte, len(agrs)+1)
		argsBuf[0] = 0
		copy(argsBuf[1:], agrs)
	}

	return RpcCall{
		objectId:     objId,
		targetClient: targetClient,
		subObjectId:  subObjectsId,
		rpcType:      rpcType,
		methodId:     methodId,
		args:         argsBuf,
	}
}

func (r *RpcCall) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(6)
	err := enc.EncodeUint(uint64(r.objectId))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.targetClient))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.subObjectId))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.rpcType))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(r.methodId))

	if err != nil {
		return err
	}

	err = enc.EncodeBytes(r.args)

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

	method, err := dec.DecodeUint16()

	if err != nil {
		return err
	}

	args, err := dec.DecodeBytes()

	r.objectId = id
	r.targetClient = clientId
	r.subObjectId = subId
	r.rpcType = typeId
	r.methodId = method
	r.args = args

	return err
}

func (r *RpcCall) GetObjectId() uint32 {
	return r.objectId
}

func (r *RpcCall) SetObjectId(oid uint32) {
	r.objectId = oid
}

func (r *RpcCall) GetTargetClient() uint32 {
	return r.targetClient
}

func (r *RpcCall) GetSubObjectId() uint16 {
	return r.subObjectId
}

func (r *RpcCall) GetRpcType() uint8 {
	return r.rpcType
}

func (r *RpcCall) GetMethodId() uint16 {
	return r.methodId
}

func (r *RpcCall) GetArgs() []byte {
	if r.args[0] == 1 {
		return r.args[3:]
	}

	return r.args[1:]
}

func (r *RpcCall) TryGetCreationId() (bool, uint16) {
	if r.args[0] == 1 {
		encoded := r.args[1:3]
		return true, binary.BigEndian.Uint16(encoded)
	}

	return false, 0
}

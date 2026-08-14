package gameentities

import (
	"encoding/binary"
	"errors"
	eventtypes "positron/game/gameHandlers/eventTypes"

	"github.com/vmihailenco/msgpack/v5"
)

type RpcCall struct {
	arenaPtr     uint32
	arenaLen     uint32
	targetClient uint32
	objectId     uint32
	methodId     uint16
	subObjectId  uint16
	rpcType      uint8
}

func NewRpcCall(ptr, dlen, objId, targetClient uint32, subObjectsId uint16, rpcType uint8, methodId uint16) RpcCall {
	return RpcCall{
		arenaPtr:     ptr,
		arenaLen:     dlen,
		targetClient: targetClient,
		objectId:     objId,
		methodId:     methodId,
		subObjectId:  subObjectsId,
		rpcType:      rpcType,
	}
}

func (r *RpcCall) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)

	if err := enc.EncodeArrayLen(7); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.arenaPtr)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.arenaLen)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.targetClient)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.objectId)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.methodId)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.subObjectId)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(r.rpcType)); err != nil {
		return err
	}

	return nil
}

func (r *RpcCall) DecodeMsgpack(dec *msgpack.Decoder) error {
	if arrLen, err := dec.DecodeArrayLen(); err != nil || arrLen != 7 {
		if arrLen != 7 {
			return errors.New("Rpc call arr invalid")
		}

		return err
	}

	if ptr, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		r.arenaPtr = ptr
	}

	if len, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		r.arenaLen = len
	}

	if targetClient, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		r.targetClient = targetClient
	}

	if objectId, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		r.objectId = objectId
	}

	if method, err := dec.DecodeUint16(); err != nil {
		return err
	} else {
		r.methodId = method
	}

	if subId, err := dec.DecodeUint16(); err != nil {
		return err
	} else {
		r.subObjectId = subId
	}

	if typeId, err := dec.DecodeUint8(); err != nil {
		return err
	} else {
		r.rpcType = typeId
	}

	return nil
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

// ptr len
func (r *RpcCall) GetDescriptors() (uint32, uint32) {
	return r.arenaPtr, r.arenaLen
}

func (r *RpcCall) SetDescriptors(ptr, len uint32) {
	r.arenaPtr = ptr
	r.arenaLen = len
}

func (r *RpcCall) MarkRpcInvalid() {
	r.rpcType = eventtypes.RPC_INVALID
}

func GetRpcArgs(args []byte) []byte {
	if args[0] == 1 {
		return args[3:]
	}

	return args[1:]
}

func DeconstructRpc(args []byte) (bool, uint16, []byte) {
	if args[0] == 1 {
		encoded := args[1:3]
		return true, binary.BigEndian.Uint16(encoded), args[3:]
	}

	return false, 0, args[1:]
}

package gameentities

import (
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type NetValue struct {
	persistentMemoryDescriptor int // this is only local server value and MUST NOT BE serialized and transfered via network

	arenaPtr       uint32
	arenaLen       uint32
	parentObjectId uint32
	valueId        uint16
	deleting       bool
}

func NewNetValueAsTransient(ptr, len, parentObj uint32, valueId uint16, isDeleting bool) NetValue {
	return NetValue{
		arenaPtr:       ptr,
		arenaLen:       len,
		parentObjectId: parentObj,
		valueId:        valueId,
		deleting:       isDeleting,
	}
}

func NewNetValueAsPersistent(persistentDescriptor int, parentObj uint32, valueId uint16, isDeleting bool) NetValue {
	return NetValue{
		persistentMemoryDescriptor: persistentDescriptor,
		parentObjectId:             parentObj,
		valueId:                    valueId,
		deleting:                   isDeleting,
	}
}

func (n *NetValue) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)

	if err := enc.EncodeArrayLen(5); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.arenaPtr)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.arenaLen)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.parentObjectId)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.valueId)); err != nil {
		return err
	}

	if err := enc.EncodeBool(n.deleting); err != nil {
		return err
	}

	return nil
}

func (n *NetValue) DecodeMsgpack(dec *msgpack.Decoder) error {
	if arrLen, err := dec.DecodeArrayLen(); err != nil || arrLen != 5 {
		if arrLen != 5 {
			return errors.New("Net value arr len invalid!")
		}

		return err
	}

	if arenaPtr, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		n.arenaPtr = arenaPtr
	}

	if arenaLen, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		n.arenaLen = arenaLen
	}

	if parentObjectId, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		n.parentObjectId = parentObjectId
	}

	if valueId, err := dec.DecodeUint16(); err != nil {
		return err
	} else {
		n.valueId = valueId
	}

	if isDeleting, err := dec.DecodeBool(); err != nil {
		return err
	} else {
		n.deleting = isDeleting
	}

	return nil
}

func (n *NetValue) GetValueId() uint16 {
	return n.valueId
}

func (n *NetValue) GetParentObjectId() uint32 {
	return n.parentObjectId
}

func (n *NetValue) GetPersistentMemoryDescriptor() int {
	return n.persistentMemoryDescriptor
}

func (n *NetValue) SetPersistentMemoryDescriptor(descriptor int) {
	n.persistentMemoryDescriptor = descriptor
}

// ptr len
func (n *NetValue) GetTransientMemoryDescriptor() (uint32, uint32) {
	return n.arenaPtr, n.arenaLen
}

func (n *NetValue) SetTransientMemoryDescriptors(ptr, len uint32) {
	n.arenaPtr = ptr
	n.arenaLen = len
}

func (n *NetValue) GetIsDeleting() bool {
	return n.deleting
}

func (n *NetValue) MarkAsDeleting() {
	n.deleting = true
}

package gameentities

import (
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type PersistentNetValue struct {
	flatArrayIdDescriptor uint32
	arenaPtr              uint32
	arenaLen              uint32
	deleting              bool
}

func NewPersistentNetValue(flatIndex, arenaPtr, arenaLen uint32, isDeleting bool) PersistentNetValue {
	return PersistentNetValue{
		flatArrayIdDescriptor: flatIndex,
		arenaPtr:              arenaPtr,
		arenaLen:              arenaLen,
		deleting:              isDeleting,
	}
}

func (n *PersistentNetValue) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)

	if err := enc.EncodeArrayLen(4); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.flatArrayIdDescriptor)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.arenaPtr)); err != nil {
		return err
	}

	if err := enc.EncodeUint(uint64(n.arenaLen)); err != nil {
		return err
	}

	if err := enc.EncodeBool(n.deleting); err != nil {
		return err
	}

	return nil
}

func (n *PersistentNetValue) DecodeMsgpack(dec *msgpack.Decoder) error {
	if arrLen, err := dec.DecodeArrayLen(); err != nil || arrLen != 4 {
		if arrLen != 4 {
			return errors.New("Net value (transient) arr len invalid!")
		}

		return err
	}

	if flatArrayId, err := dec.DecodeUint32(); err != nil {
		return err
	} else {
		n.flatArrayIdDescriptor = flatArrayId
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

	if isDeleting, err := dec.DecodeBool(); err != nil {
		return err
	} else {
		n.deleting = isDeleting
	}

	return nil
}

func (n *PersistentNetValue) GetArrayDescriptor() uint32 {
	return n.flatArrayIdDescriptor
}

func (n *PersistentNetValue) SetArrayDescriptor(flatArrayIdDescriptor uint32) {
	n.flatArrayIdDescriptor = flatArrayIdDescriptor
}

// ptr len
func (n *PersistentNetValue) GetTransientMemoryDescriptor() (uint32, uint32) {
	return n.arenaPtr, n.arenaLen
}

func (n *PersistentNetValue) SetTransientMemoryDescriptors(ptr, len uint32) {
	n.arenaPtr = ptr
	n.arenaLen = len
}

func (n *PersistentNetValue) GetIsDeleting() bool {
	return n.deleting
}

func (n *PersistentNetValue) MarkAsDeleting() {
	n.deleting = true
}

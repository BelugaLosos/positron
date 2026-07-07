package gameentities

import (
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type NetValue struct {
	payload        []byte
	parentObjectId uint32
	valueId        uint16
	deleting       bool
}

func NewNetValue(payload []byte, parentObj uint32, valueId uint16, isDeleting bool) *NetValue {
	return &NetValue{
		payload:        payload,
		parentObjectId: parentObj,
		valueId:        valueId,
		deleting:       isDeleting,
	}
}

func (n *NetValue) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(4)
	err := enc.EncodeUint(uint64(n.valueId))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(n.parentObjectId))

	if err != nil {
		return err
	}

	err = enc.EncodeBool(n.deleting)

	if err != nil {
		return err
	}

	err = enc.EncodeBytes(n.payload)

	if err != nil {
		return err
	}

	return nil
}

func (n *NetValue) DecodeMsgpack(dec *msgpack.Decoder) error {
	arrLen, arrErr := dec.DecodeArrayLen()
	valueId, err := dec.DecodeUint16()

	if arrErr != nil {
		return arrErr
	}

	if arrLen != 4 {
		return errors.New("Net value arr len invalid!")
	}

	if err != nil {
		return err
	}

	parentObjectId, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	isDeleting, err := dec.DecodeBool()

	if err != nil {
		return err
	}

	paylpad, err := dec.DecodeBytes()

	n.valueId = valueId
	n.parentObjectId = parentObjectId
	n.deleting = isDeleting
	n.payload = paylpad

	return err
}

func (n *NetValue) GetValueId() uint16 {
	return n.valueId
}

func (n *NetValue) GetParentObjectId() uint32 {
	return n.parentObjectId
}

func (n *NetValue) GetPayload() []byte {
	return n.payload
}

func (n *NetValue) ModifyPayload(newPayload []byte) {
	n.payload = newPayload
}

func (n *NetValue) GetIsDeleting() bool {
	return n.deleting
}

func (n *NetValue) MarkAsDeleting() {
	n.deleting = true
}

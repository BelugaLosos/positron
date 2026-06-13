package gameentities

import (
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type NetValue struct {
	ParentObjectId uint32
	SubObjectId    uint16
	ValueId        uint16
	Deleting       bool
	Payload        []byte
}

func (n *NetValue) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(5)
	err := enc.EncodeUint(uint64(n.ValueId))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(n.ParentObjectId))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(n.SubObjectId))

	if err != nil {
		return err
	}

	err = enc.EncodeBool(n.Deleting)

	if err != nil {
		return err
	}

	err = enc.EncodeBytes(n.Payload)

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

	if arrLen != 5 {
		return errors.New("Net value arr len invalid!")
	}

	if err != nil {
		return err
	}

	parentObjectId, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	subObjectId, err := dec.DecodeUint16()

	if err != nil {
		return err
	}

	isDeleting, err := dec.DecodeBool()

	if err != nil {
		return err
	}

	paylpad, err := dec.DecodeBytes()

	n.ValueId = valueId
	n.ParentObjectId = parentObjectId
	n.SubObjectId = subObjectId
	n.Deleting = isDeleting
	n.Payload = paylpad

	return err
}

func (n *NetValue) GetValueId() uint16 {
	return n.ValueId
}

func (n *NetValue) GetParentObjectId() uint32 {
	return n.ParentObjectId
}

func (n *NetValue) GetSubObjectId() uint16 {
	return n.SubObjectId
}

func (n *NetValue) GetPayload() []byte {
	return n.Payload
}

func (n *NetValue) ModifyPayload(newPayload []byte) {
	n.Payload = newPayload
}

func (n *NetValue) GetIsDeleting() bool {
	return n.Deleting
}

func (n *NetValue) MarkAsDeleting() {
	n.Deleting = true
}

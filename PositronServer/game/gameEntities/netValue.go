package gameentities

import "github.com/vmihailenco/msgpack/v5"

type NetValue struct {
	_msgpack struct{} `msgpack:",as_array"`

	ValueId        uint64
	ParentObjectId uint32
	SubObjectId    uint16
	Deleting       bool
	Payload        []byte
}

func (n *NetValue) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.EncodeArrayLen(5)
	err := enc.EncodeUint64(n.ValueId)

	if err != nil {
		return err
	}

	err = enc.EncodeUint32(n.ParentObjectId)

	if err != nil {
		return err
	}

	err = enc.EncodeUint16(n.SubObjectId)

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
	dec.DecodeArrayLen()
	valueId, err := dec.DecodeUint64()

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

func (n *NetValue) GetValueId() uint64 {
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

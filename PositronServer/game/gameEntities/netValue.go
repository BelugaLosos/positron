package gameentities

type NetValue struct {
	_msgpack struct{} `msgpack:",as_array"`

	ValueId        uint64
	ParentObjectId uint32
	SubObjectId    uint16
	Deleting       bool
	Payload        []byte
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

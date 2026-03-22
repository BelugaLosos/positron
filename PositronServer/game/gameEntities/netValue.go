package gameentities

type NetValue struct {
	_msgpack struct{} `msgpack:",as_array"`

	valueId        uint64
	parentObjectId uint32
	subObjectId    uint16
	deleting       bool
	payload        []byte
}

func (n *NetValue) GetValueId() uint64 {
	return n.valueId
}

func (n *NetValue) GetParentObjectId() uint32 {
	return n.parentObjectId
}

func (n *NetValue) GetSubObjectId() uint16 {
	return n.subObjectId
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

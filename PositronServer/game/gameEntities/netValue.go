package gameentities

type NetValue struct {
	creationId     uint64
	parentObjectId uint32
	subObjectId    uint16
	payload        []byte
}

func (n *NetValue) GetCreationId() uint64 {
	return n.creationId
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

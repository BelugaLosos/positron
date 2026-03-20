package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	host              uint32
	client            uint32
	newObjects        []*gameentities.GameObject
	removedObjects    []uint32
	transferedObjects []uint32
	valueMod          []*gameentities.NetValue
	rpc               []*gameentities.RpcCall
}

func NewTickPacket(host uint32, sourceClient uint32, newObjects []*gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, valueMod []*gameentities.NetValue, rpc []*gameentities.RpcCall) *GameTickPacket {
	return &GameTickPacket{
		host:              host,
		client:            sourceClient,
		newObjects:        newObjects,
		removedObjects:    removedObjects,
		transferedObjects: transferedObjects,
		valueMod:          valueMod,
		rpc:               rpc,
	}
}

func (g *GameTickPacket) GetHost() uint32 {
	return g.host
}

func (g *GameTickPacket) GetSourceClient() uint32 {
	return g.client
}

func (g *GameTickPacket) GetNewObjects() []*gameentities.GameObject {
	return g.newObjects
}

func (g *GameTickPacket) GetRemovedObjects() []uint32 {
	return g.removedObjects
}

func (g *GameTickPacket) GetTranferedObjects() []uint32 {
	return g.transferedObjects
}

func (g *GameTickPacket) GetValueMod() []*gameentities.NetValue {
	return g.valueMod
}

func (g *GameTickPacket) GetRpcs() []*gameentities.RpcCall {
	return g.rpc
}

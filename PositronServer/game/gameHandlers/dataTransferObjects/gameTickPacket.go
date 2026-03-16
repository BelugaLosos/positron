package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	host           uint32
	newObjects     []*gameentities.GameObject
	removedObjects []uint32
	valueMod       []*gameentities.NetValue
	rpc            []*gameentities.RpcCall
}

func NewTickPacket(host uint32, newObjects []*gameentities.GameObject, removedObjects []uint32, valueMod []*gameentities.NetValue, rpc []*gameentities.RpcCall) *GameTickPacket {
	return &GameTickPacket{
		host:           host,
		newObjects:     newObjects,
		removedObjects: removedObjects,
		valueMod:       valueMod,
		rpc:            rpc,
	}
}

func (g *GameTickPacket) GetNewObjects() []*gameentities.GameObject {
	return g.newObjects
}

func (g *GameTickPacket) GetRemovedObjects() []uint32 {
	return g.removedObjects
}

func (g *GameTickPacket) GetValueMod() []*gameentities.NetValue {
	return g.valueMod
}

func (g *GameTickPacket) GetRpcs() []*gameentities.RpcCall {
	return g.rpc
}

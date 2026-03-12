package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	newObjects     []gameentities.GameObject
	removedObjects []uint32
	valueMod       []gameentities.NetValue
	rpc            []gameentities.RpcCall
}

func (g *GameTickPacket) GetNewObjects() []gameentities.GameObject {
	return g.newObjects
}

func (g *GameTickPacket) GetRemovedObjects() []uint32 {
	return g.removedObjects
}

func (g *GameTickPacket) GetValueMod() []gameentities.NetValue {
	return g.valueMod
}

func (g *GameTickPacket) GetRpcs() []gameentities.RpcCall {
	return g.rpc
}

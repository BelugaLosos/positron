package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	Host              uint32
	Client            uint32
	NewObjects        []*gameentities.GameObject
	RemovedObjects    []uint32
	TransferedObjects []uint32
	ValueMod          []*gameentities.NetValue
	Rpc               []*gameentities.RpcCall
}

func NewTickPacket(host uint32, sourceClient uint32, newObjects []*gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, valueMod []*gameentities.NetValue, rpc []*gameentities.RpcCall) *GameTickPacket {
	return &GameTickPacket{
		Host:              host,
		Client:            sourceClient,
		NewObjects:        newObjects,
		RemovedObjects:    removedObjects,
		TransferedObjects: transferedObjects,
		ValueMod:          valueMod,
		Rpc:               rpc,
	}
}

func (g *GameTickPacket) GetHost() uint32 {
	return g.Host
}

func (g *GameTickPacket) GetSourceClient() uint32 {
	return g.Client
}

func (g *GameTickPacket) GetNewObjects() []*gameentities.GameObject {
	return g.NewObjects
}

func (g *GameTickPacket) GetRemovedObjects() []uint32 {
	return g.RemovedObjects
}

func (g *GameTickPacket) GetTranferedObjects() []uint32 {
	return g.TransferedObjects
}

func (g *GameTickPacket) GetValueMod() []*gameentities.NetValue {
	return g.ValueMod
}

func (g *GameTickPacket) GetRpcs() []*gameentities.RpcCall {
	return g.Rpc
}

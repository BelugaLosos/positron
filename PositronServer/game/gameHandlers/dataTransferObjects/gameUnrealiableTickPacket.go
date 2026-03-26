package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameUnreliableTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	sourceClient uint32
	movedObjects []*gameentities.Tranform
}

func NewGameUnreliableTickPacket(movedObjects []*gameentities.Tranform, sourceClient uint32) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		sourceClient: sourceClient,
		movedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []*gameentities.Tranform {
	return g.movedObjects
}

func (g *GameUnreliableTickPacket) GetSourceClient() uint32 {
	return g.sourceClient
}

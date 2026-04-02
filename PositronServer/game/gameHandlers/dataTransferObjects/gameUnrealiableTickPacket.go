package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameUnreliableTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	SourceClient uint32
	MovedObjects []*gameentities.Tranform
}

func NewGameUnreliableTickPacket(movedObjects []*gameentities.Tranform, sourceClient uint32) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		SourceClient: sourceClient,
		MovedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []*gameentities.Tranform {
	return g.MovedObjects
}

func (g *GameUnreliableTickPacket) GetSourceClient() uint32 {
	return g.SourceClient
}

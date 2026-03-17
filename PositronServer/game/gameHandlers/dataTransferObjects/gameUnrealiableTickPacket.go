package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameUnreliableTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	timestamp    uint64
	movedObjects []*gameentities.Tranform
}

func NewGameUnreliableTickPacket(movedObjects []*gameentities.Tranform, timeStamp uint64) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		timestamp:    timeStamp,
		movedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []*gameentities.Tranform {
	return g.movedObjects
}

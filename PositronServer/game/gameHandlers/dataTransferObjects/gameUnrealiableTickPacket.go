package datatransferobjects

import gameentities "positron/game/gameEntities"

type GameUnreliableTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	timestamp    uint64
	movedObjects []gameentities.GameObject
}

func NewGameUnreliableTickPacket(movedObjects []gameentities.GameObject, timeStamp uint64) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		timestamp:    timeStamp,
		movedObjects: movedObjects,
	}
}

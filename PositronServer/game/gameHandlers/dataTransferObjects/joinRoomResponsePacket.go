package datatransferobjects

import gameentities "positron/game/gameEntities"

type JoinRoomResponsePacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	gameObjects []*gameentities.GameObject
	netValues   []*gameentities.NetValue
	cachedRpcs  []*gameentities.RpcCall
	selfId      uint32
	host        uint32
}

func NewJoinRoomResponsePacket(gameObjects []*gameentities.GameObject, netValues []*gameentities.NetValue, cachedRpcs []*gameentities.RpcCall, selfId uint32, host uint32) *JoinRoomResponsePacket {
	return &JoinRoomResponsePacket{
		gameObjects: gameObjects,
		netValues:   netValues,
		cachedRpcs:  cachedRpcs,
		selfId:      selfId,
		host:        host,
	}
}

package datatransferobjects

import gameentities "positron/game/gameEntities"

type JoinRoomResponsePacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	GameObjects []*gameentities.GameObject
	NetValues   []*gameentities.NetValue
	CachedRpcs  []*gameentities.RpcCall
	Tickrate    uint32
	SelfId      uint32
	Host        uint32
}

func NewJoinRoomResponsePacket(gameObjects []*gameentities.GameObject, netValues []*gameentities.NetValue, cachedRpcs []*gameentities.RpcCall, tickrate uint32, selfId uint32, host uint32) *JoinRoomResponsePacket {
	return &JoinRoomResponsePacket{
		GameObjects: gameObjects,
		NetValues:   netValues,
		CachedRpcs:  cachedRpcs,
		Tickrate:    tickrate,
		SelfId:      selfId,
		Host:        host,
	}
}

package gamehandlers

import (
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type PingHandler struct {
	transport      internal.PositronTransportServer
	uuid           string
	cachedResponse []byte
}

func NewPingHanler() *PingHandler {
	return &PingHandler{
		cachedResponse: []byte{0xFF},
	}
}

func (p *PingHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	p.transport = transport
	p.uuid = connectionUuid
}

func (p *PingHandler) GetType() byte {
	return eventtypes.PING
}

func (p *PingHandler) PassHandle(packet []byte) {
	p.transport.SendToPeer(p.cachedResponse, eventtypes.PONG, p.uuid, true)
}

func (p *PingHandler) SetRoom(room *room.Room) {}

package gamehandlers

import (
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type PingHandler struct {
	transport internal.PositronTransportServer
	uuid      string
}

func NewPingHanler() *PingHandler {
	return &PingHandler{}
}

func (p *PingHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	p.transport = transport
	p.uuid = connectionUuid
}

func (p *PingHandler) GetType() byte {
	return eventtypes.PING
}

func (p *PingHandler) PassHandle(packet []byte) {
	p.transport.SendToPeer([]byte{0x0}, eventtypes.PONG, p.uuid)
}

func (p *PingHandler) SetRoom(room *room.Room) {}

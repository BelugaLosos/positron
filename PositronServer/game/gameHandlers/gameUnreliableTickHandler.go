package gamehandlers

import (
	"log"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type GameUnreliableTickHandler struct {
	transport  internal.PositronTransportServer
	uuid       string
	room       *room.Room
	clientId   uint32
	marshaller internal.MarshalService
}

func NewGameUnreliableTickHandler() *GameUnreliableTickHandler {
	return &GameUnreliableTickHandler{}
}

func (g *GameUnreliableTickHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	g.transport = transport
	g.uuid = connectionUuid
	g.marshaller = gServer.GetMarshaller()
}

func (g *GameUnreliableTickHandler) GetType() byte {
	return eventtypes.UNRELIABLE_TICK
}

func (g *GameUnreliableTickHandler) PassHandle(packet []byte) {
	if g.room == nil {
		return
	}

	var tickPacket datatransferobjects.GameUnreliableTickPacket
	err := g.marshaller.Unmarshal(packet, tickPacket)

	if err != nil {
		log.Println(err)
		return
	}

	if tickPacket.GetSourceClient() != g.clientId {
		log.Printf("Unrealiable update clientId spoofing detected. From %v to %v", g.clientId, tickPacket.GetSourceClient())
		g.transport.KickClient(g.uuid)
		return
	}

	g.room.ProcessUnreliableTick(&tickPacket)
}

func (g *GameUnreliableTickHandler) SetRoom(room *room.Room, inRoomId uint32) {
	g.room = room
	g.clientId = inRoomId
}

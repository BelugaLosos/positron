package gamehandlers

import (
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type GameTickHandler struct {
	transport  internal.PositronTransportServer
	uuid       string
	room       *room.Room
	clientId   uint32
	marsahller internal.MarshalService
}

func NewGameTickHandler() *GameTickHandler {
	return &GameTickHandler{}
}

func (g *GameTickHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	g.transport = transport
	g.uuid = connectionUuid
	g.marsahller = gServer.GetMarshaller()
}

func (g *GameTickHandler) GetType() byte {
	return eventtypes.TICK
}

func (g *GameTickHandler) PassHandle(packet []byte) {
	if g.room == nil {
		return
	}

	var tickPacket datatransferobjects.GameTickPacket
	err := g.marsahller.Unmarshal(packet, &tickPacket)

	if err != nil {
		log.Println(err)
		return
	}

	if tickPacket.GetSourceClient() != g.clientId {
		log.Printf("Spoofing of client id detected. From %v spoofed to %v", g.clientId, tickPacket.GetSourceClient())
		g.transport.KickClient(g.uuid)
		return
	}

	g.room.ProcessTick(&tickPacket)
}

func (g *GameTickHandler) SetRoom(room *room.Room, inRoomId uint32) {
	g.room = room
	g.clientId = inRoomId
}

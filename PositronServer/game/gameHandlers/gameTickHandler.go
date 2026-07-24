package gamehandlers

import (
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type GameTickHandler struct {
	transport                 internal.PositronTransportServer
	uuid                      string
	room                      *room.Room
	clientId                  uint32
	marsahller                internal.MarshalService
	cachedTickPacketContainer *datatransferobjects.GameTickPacket
}

func NewGameTickHandler() *GameTickHandler {
	return &GameTickHandler{}
}

func (g *GameTickHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	g.transport = transport
	g.uuid = connectionUuid
	g.marsahller = gServer.GetMarshaller()
	g.cachedTickPacketContainer = &datatransferobjects.GameTickPacket{}
}

func (g *GameTickHandler) GetType() byte {
	return eventtypes.TICK
}

func (g *GameTickHandler) PassHandle(packet []byte) {
	if g.room == nil {
		return
	}

	err := g.marsahller.Unmarshal(packet, g.cachedTickPacketContainer)

	if err != nil {
		log.Println(err)
		return
	}

	if g.cachedTickPacketContainer.GetSourceClient() != g.clientId {
		log.Printf("Spoofing of client id detected. From %v spoofed to %v", g.clientId, g.cachedTickPacketContainer.GetSourceClient())
		g.transport.KickClient(g.uuid)
		return
	}

	g.room.ProcessTick(g.cachedTickPacketContainer)
}

func (g *GameTickHandler) SetRoom(room *room.Room, inRoomId uint32) {
	g.room = room
	g.clientId = inRoomId
}

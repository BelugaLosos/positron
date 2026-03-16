package gamehandlers

import (
	"log"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type GetAllRoomsHandler struct {
	transport  internal.PositronTransportServer
	uuid       string
	gameServer internal.GameServerAdaper
	inRoomNow  bool
}

func NewGetAllRoomsHandler() *GetAllRoomsHandler {
	return &GetAllRoomsHandler{}
}

func (g *GetAllRoomsHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	g.transport = transport
	g.uuid = connectionUuid
	g.gameServer = gServer
}

func (g *GetAllRoomsHandler) GetType() byte {
	return eventtypes.GET_ALL_ROOMS
}

func (g *GetAllRoomsHandler) PassHandle(packet []byte) {
	if g.inRoomNow {
		return
	}

	rooms := g.gameServer.GetAllRooms()
	roomList := make([]*datatransferobjects.RoomsListElement, len(rooms))

	for i := range rooms {
		roomList[i] = &datatransferobjects.RoomsListElement{
			Name:           rooms[i].GetName(),
			Uuid:           rooms[i].GetUuid(),
			CurrentPlayers: rooms[i].GetCurrentConnectedPeersCount(),
			MaxPlayers:     rooms[i].GetMaxPlayersCount(),
		}
	}

	roomListDto := &datatransferobjects.RoomsListResponse{
		ListElements: roomList,
	}

	marsahlled, err := g.gameServer.GetMarshaller().Marshal(roomListDto)

	if err != nil {
		log.Println(err)
		return
	}

	err = g.transport.SendToPeer(marsahlled, eventtypes.ROOMS_LIST, g.uuid, true)

	if err != nil {
		log.Println(err)
	}
}

func (g *GetAllRoomsHandler) SetRoom(room *room.Room, inRoomId uint32) {
	g.inRoomNow = room != nil
}

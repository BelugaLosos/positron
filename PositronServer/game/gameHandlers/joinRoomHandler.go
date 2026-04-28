package gamehandlers

import (
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type JoinRoomHandler struct {
	transport  internal.PositronTransportServer
	gameServer internal.GameServerAdaper
	uuid       string
	room       *room.Room
}

func NewJoinRoomHandler() *JoinRoomHandler {
	return &JoinRoomHandler{}
}

func (j *JoinRoomHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	j.gameServer = gServer
	j.transport = transport
	j.uuid = connectionUuid
}

func (j *JoinRoomHandler) GetType() byte {
	return eventtypes.JOIN_ROOM
}

func (j *JoinRoomHandler) PassHandle(packet []byte) {
	var request datatransferobjects.JoinRoomRequestPacket
	err := j.gameServer.GetMarshaller().Unmarshal(packet, &request)

	if err != nil {
		log.Println(err)
		return
	}

	room := j.gameServer.GetRoom(request.GetTargetUuid())

	if room == nil {
		log.Printf("Attempt to join not created room %s", request.GetTargetUuid())
		return
	}

	selfId, err := room.AddPeer(j.uuid)

	if err != nil {
		log.Println(err)
		return
	}

	allHandlers := j.transport.GetPeerHandlers(j.uuid)

	for i := range allHandlers {
		allHandlers[i].SetRoom(room, selfId)
	}

	gos, values, rpcs := j.room.GetWorld()

	response := datatransferobjects.NewJoinRoomResponsePacket(gos, values, rpcs, uint32(room.GetTickrate()), selfId, room.GetHost(), room.GetScene())
	binaryResponse, err := j.gameServer.GetMarshaller().Marshal(response)

	if err != nil {
		log.Println(err)
		return
	}

	err = j.transport.SendToPeer(binaryResponse, eventtypes.ROOM_JOINED, j.uuid, true)

	if err != nil {
		log.Println(err)
	}
}

func (j *JoinRoomHandler) SetRoom(room *room.Room, inRoomId uint32) {
	j.room = room
}

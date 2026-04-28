package gamehandlers

import (
	"log"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type LeaveRoomHandler struct {
	transport      internal.PositronTransportServer
	uuid           string
	cachedResponse []byte
	room           *room.Room
}

func NewLeaveRoomHandler() *LeaveRoomHandler {
	return &LeaveRoomHandler{
		cachedResponse: []byte{0xFF},
	}
}

func (l *LeaveRoomHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	l.transport = transport
	l.uuid = connectionUuid
}

func (l *LeaveRoomHandler) GetType() byte {
	return eventtypes.ROOM_LEAVE
}

func (l *LeaveRoomHandler) PassHandle(packet []byte) {
	if l.room == nil {
		log.Printf("Can`t leave room outside any room C_UUID: %s", l.uuid)
		return
	}

	l.room.RemovePeer(l.uuid)
	err := l.transport.SendToPeer(l.cachedResponse, eventtypes.ROOM_DISCONNECTED, l.uuid, true)

	if err != nil {
		log.Println(err)
	} else {
		log.Printf("Client %s left room with guid %s", l.uuid, l.room.GetUuid())
	}
}

func (l *LeaveRoomHandler) SetRoom(room *room.Room, inRoomId uint32) {
	l.room = room
}

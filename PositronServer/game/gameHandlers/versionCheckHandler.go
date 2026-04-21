package gamehandlers

import (
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
)

type VersionCheckHandler struct {
	transport internal.PositronTransportServer
	uuid      string
	game      internal.GameServerAdaper
}

func NewVersionCheckHandler() *VersionCheckHandler {
	return &VersionCheckHandler{}
}

func (v *VersionCheckHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	v.transport = transport
	v.uuid = connectionUuid
	v.game = gServer
}

func (v *VersionCheckHandler) GetType() byte {
	return eventtypes.VERSION_CHECK_REQUEST
}

func (v *VersionCheckHandler) PassHandle(packet []byte) {
	isValid := uint8(0)

	if string(packet) == v.game.GetVersion() {
		isValid = 1
	}

	v.transport.SendToPeer([]byte{isValid}, eventtypes.VERSION_CHECK_RESPONSE, v.uuid, true)
}

func (v *VersionCheckHandler) SetRoom(room *room.Room, inRoomId uint32) {}

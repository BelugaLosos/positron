package internal

import "positron/game/room"

type GameServerAdaper interface {
	GetRoom(roomUuid string) *room.Room
	CreateRoom(maxSlots int) string
	GetMarshaller() MarshalService
}

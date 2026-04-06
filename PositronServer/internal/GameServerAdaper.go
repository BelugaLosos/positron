package internal

import (
	"positron/game/room"
	"time"
)

type GameServerAdaper interface {
	GetRoom(roomUuid string) *room.Room
	GetAllRooms() []*room.Room
	CreateRoom(name string, maxSlots int, ttl time.Duration, scene uint32, tickrate uint32, externalData []byte) string
	GetMarshaller() MarshalService
}

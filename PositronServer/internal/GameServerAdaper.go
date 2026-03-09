package internal

import (
	"positron/game/room"
	"time"
)

type GameServerAdaper interface {
	GetRoom(roomUuid string) *room.Room
	CreateRoom(name string, maxSlots int, ttl time.Duration) string
	GetMarshaller() MarshalService
}

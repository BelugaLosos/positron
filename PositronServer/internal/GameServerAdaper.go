package internal

import (
	"positron/game/room"
	"time"
)

type GameServerAdaper interface {
	GetRoom(roomUuid string) *room.Room
	CreateRoom(maxSlots int, ttl time.Duration) string
	GetMarshaller() MarshalService
}

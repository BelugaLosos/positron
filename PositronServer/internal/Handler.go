package internal

import "positron/game/room"

type Handler interface {
	Init(transport PositronTransportServer, gServer GameServerAdaper, connectionUuid string)
	GetType() byte
	PassHandle(packet []byte)
	SetRoom(room *room.Room)
}

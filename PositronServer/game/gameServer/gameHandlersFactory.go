package gameserver

import (
	gamehandlers "positron/game/gameHandlers"
	"positron/internal"
)

type gameHandlersFactory struct {
	gServer internal.GameServerAdaper
}

func NewGameHandlersFactory(gServer *GameServer) *gameHandlersFactory {
	return &gameHandlersFactory{
		gServer: gServer,
	}
}

func (g *gameHandlersFactory) Create() []internal.Handler {
	handlers := make([]internal.Handler, 0)
	handlers = append(handlers, gamehandlers.NewPingHanler())
	handlers = append(handlers, gamehandlers.NewCreateRoomHandler())
	handlers = append(handlers, gamehandlers.NewGetAllRoomsHandler())

	return handlers
}

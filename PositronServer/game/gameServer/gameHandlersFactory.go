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

func (g *gameHandlersFactory) Create() ([]internal.Handler, internal.Handler) {
	disconnectionHandler := gamehandlers.NewLeaveRoomHandler()

	handlers := make([]internal.Handler, 0)
	handlers = append(handlers, gamehandlers.NewPingHanler())
	handlers = append(handlers, gamehandlers.NewCreateRoomHandler())
	handlers = append(handlers, gamehandlers.NewGetAllRoomsHandler())
	handlers = append(handlers, gamehandlers.NewJoinRoomHandler())
	handlers = append(handlers, disconnectionHandler)
	handlers = append(handlers, gamehandlers.NewGameTickHandler())
	handlers = append(handlers, gamehandlers.NewGameUnreliableTickHandler())

	return handlers, disconnectionHandler
}

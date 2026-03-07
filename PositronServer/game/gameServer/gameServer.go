package gameserver

import (
	"log"
	"positron/game/room"
	"positron/internal"
	"sync"
)

type GameServer struct {
	mutex *sync.RWMutex

	addr            string
	transport       internal.PositronTransportServer
	handlersFactory internal.HandlersFactory
	marhaller       internal.MarshalService

	rooms map[string]*room.Room
}

func NewGameServer(addr string, transport internal.PositronTransportServer, marshaller internal.MarshalService) *GameServer {
	server := &GameServer{
		addr:            addr,
		transport:       transport,
		handlersFactory: nil,
		marhaller:       marshaller,
		rooms:           make(map[string]*room.Room),
	}

	server.handlersFactory = NewGameHandlersFactory(server)

	return server
}

func (g *GameServer) Start(wg *sync.WaitGroup) error {
	log.Println("Positron started succesfully !")

	return g.transport.Start(g.addr, g.handlersFactory, g, wg)
}

func (g *GameServer) Stop() error {
	return g.transport.Stop()
}

func (g *GameServer) GetRoom(roomUuid string) *room.Room {
	g.mutex.RLock()
	defer g.mutex.RUnlock()

	return g.rooms[roomUuid]
}

func (g *GameServer) CreateRoom(maxSlots int) string {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	room := room.NewRoom(maxSlots)
	g.rooms[room.GetUuid()] = room

	return room.GetUuid()
}

func (g *GameServer) GetMarshaller() internal.MarshalService {
	return g.marhaller
}

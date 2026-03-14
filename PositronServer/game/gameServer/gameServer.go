package gameserver

import (
	"log"
	"positron/game/room"
	"positron/internal"
	"sync"
	"time"
)

type GameServer struct {
	mutex       *sync.RWMutex
	termination chan interface{}

	addr            string
	transport       internal.PositronTransportServer
	handlersFactory internal.HandlersFactory
	marhaller       internal.MarshalService

	rooms map[string]*room.Room
}

func NewGameServer(addr string, transport internal.PositronTransportServer, marshaller internal.MarshalService) *GameServer {
	server := &GameServer{
		mutex:           &sync.RWMutex{},
		termination:     make(chan interface{}),
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

	go g.filterEmptyRooms()
	return g.transport.Start(g.addr, g.handlersFactory, g, wg)
}

func (g *GameServer) Stop() error {
	close(g.termination)
	return g.transport.Stop()
}

func (g *GameServer) GetRoom(roomUuid string) *room.Room {
	g.mutex.RLock()
	defer g.mutex.RUnlock()

	return g.rooms[roomUuid]
}

func (g *GameServer) GetAllRooms() []*room.Room {
	g.mutex.RLock()
	defer g.mutex.RUnlock()

	rooms := make([]*room.Room, 0)

	for _, room := range g.rooms {
		rooms = append(rooms, room)
	}

	return rooms
}

func (g *GameServer) CreateRoom(name string, maxSlots int, ttl time.Duration) string {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	room := room.NewRoom(name, maxSlots, ttl)
	g.rooms[room.GetUuid()] = room

	return room.GetUuid()
}

func (g *GameServer) GetMarshaller() internal.MarshalService {
	return g.marhaller
}

func (g *GameServer) filterEmptyRooms() {
	for {
		select {
		case <-g.termination:
			return
		default:
			for roomUuid, room := range g.rooms {
				if room.GetCurrentConnectedPeersCount() == 0 && room.IsTimeFromLastLeaveOverTTL() {
					delete(g.rooms, roomUuid)
				}
			}
		}

		time.Sleep(10 * time.Second)
	}
}

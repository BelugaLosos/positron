package gameserver

import (
	"log"
	eventtypes "positron/game/gameHandlers/eventTypes"
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

	go g.roomTick(room)

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
			g.mutex.Lock()

			for roomUuid, room := range g.rooms {
				if room.GetCurrentConnectedPeersCount() == 0 && room.IsTimeFromLastLeaveOverTTL() {
					close(room.Termination)
					delete(g.rooms, roomUuid)
				}
			}

			g.mutex.Unlock()
		}

		time.Sleep(10 * time.Second)
	}
}

func (g *GameServer) roomTick(room *room.Room) {
	for {
		select {
		case <-room.Termination:
			log.Printf("Room %s disposed", room.GetUuid())
			return
		default:
			packet, unreliablePacket := room.CreateTickPackets()
			peers := room.GetAllConnectedPeers()

			for i := range peers {
				packetMarshalled, err := g.marhaller.Marshal(packet)                 //EXTRA ALLOC!
				packetUnrMarshalled, unrErr := g.marhaller.Marshal(unreliablePacket) //EXTRA ALLOC!

				if err == nil {
					g.transport.SendToPeer(packetMarshalled, eventtypes.TICK, peers[i], true)
				} else {
					log.Println(err)
				}

				if unrErr == nil {
					g.transport.SendToPeer(packetUnrMarshalled, eventtypes.UNRELIABLE_TICK, peers[i], false)
				} else {
					log.Println(unrErr)
				}
			}

			room.ResetTempBuffers()

			time.Sleep((1 * time.Second) / time.Duration(room.GetTickrate()))
		}
	}
}

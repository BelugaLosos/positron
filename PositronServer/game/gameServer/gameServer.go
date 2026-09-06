package gameserver

import (
	"bytes"
	"log"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
	networkio "positron/internal/networkIo"
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

	gameVersion string

	rooms               map[string]*room.Room
	lastRoomUuidPostfix int

	retransmitStaticObjectsLimit           int
	retransmitStaticObjectsFramesThrashold int
	retransmissionForceDisabled            bool
}

var bufferPool = sync.Pool{
	New: func() interface{} {
		return &bytes.Buffer{}
	},
}

var rawBuffersPool = sync.Pool{
	New: func() interface{} {
		return make([]byte, 4096)
	},
}

func NewGameServer(addr string, transport internal.PositronTransportServer, marshaller internal.MarshalService, version string, rtLimit, rtThrashold int, rtDisable bool) *GameServer {
	server := &GameServer{
		mutex:                                  &sync.RWMutex{},
		termination:                            make(chan interface{}),
		addr:                                   addr,
		transport:                              transport,
		handlersFactory:                        nil,
		marhaller:                              marshaller,
		gameVersion:                            version,
		rooms:                                  make(map[string]*room.Room),
		lastRoomUuidPostfix:                    0,
		retransmitStaticObjectsLimit:           rtLimit,
		retransmitStaticObjectsFramesThrashold: rtThrashold,
		retransmissionForceDisabled:            rtDisable,
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

func (g *GameServer) CreateRoom(name string, maxSlots int32, ttl time.Duration, scene uint32, tickrate uint32, externalData []byte) string {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	room := room.NewRoom(name, maxSlots, ttl, scene, tickrate, externalData, g.retransmitStaticObjectsLimit, g.retransmitStaticObjectsFramesThrashold, g.retransmissionForceDisabled)
	room.AddPostfixToUuid(g.lastRoomUuidPostfix)
	g.lastRoomUuidPostfix++

	g.rooms[room.GetUuid()] = room

	go g.roomTick(room)

	return room.GetUuid()
}

func (g *GameServer) GetMarshaller() internal.MarshalService {
	return g.marhaller
}

func (g *GameServer) GetVersion() string {
	return g.gameVersion
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
	room.RecordStartupTimeOnClock()
	writer := networkio.NewNetworkWriter()

	for {
		select {
		case <-room.Termination:
			log.Printf("Room %s disposed", room.GetUuid())
			return
		case <-room.Ticker.C:
			packetMarshallBuffer := bufferPool.Get().(*bytes.Buffer)
			packetArensBuf := rawBuffersPool.Get().([]byte)
			packetUnrMarshalled := bufferPool.Get().(*bytes.Buffer)

			packetMarshallBuffer.Reset()
			packetUnrMarshalled.Reset()

			room.Lock()

			packet, unreliablePacket, netValuesArena, rpcsArena := room.CreateTickPackets()
			peers := room.GetAllConnectedPeers()

			err := g.marhaller.MarshalNonAlloc(packetMarshallBuffer, packet)

			targetLen := 16 + packetMarshallBuffer.Len() + len(netValuesArena) + len(rpcsArena)
			packetArensBuf = checkAndResizeBuffer(packetArensBuf, targetLen)[:targetLen]

			writer.Wrap(packetArensBuf)

			writer.WriteUint32(uint32(packetMarshallBuffer.Len()))
			writer.WriteSegment(packetMarshallBuffer.Bytes())

			writer.WriteUint32(uint32(len(netValuesArena)))
			writer.WriteSegment(netValuesArena)

			writer.WriteUint32(uint32(len(rpcsArena)))
			writer.WriteSegment(rpcsArena)

			unrErr := g.marhaller.MarshalNonAlloc(packetUnrMarshalled, unreliablePacket)

			room.ResetTempBuffers()

			room.Unlock()

			for i := range peers {
				if err == nil {
					g.transport.SendToPeer(packetArensBuf, eventtypes.TICK, peers[i], true)
				} else {
					log.Println(err)
				}

				if unrErr == nil {
					g.transport.SendToPeer(packetUnrMarshalled.Bytes(), eventtypes.UNRELIABLE_TICK, peers[i], false)
				} else {
					log.Println(unrErr)
				}
			}

			writer.Free()
			bufferPool.Put(packetMarshallBuffer)
			rawBuffersPool.Put(packetArensBuf)
			bufferPool.Put(packetUnrMarshalled)
		}
	}
}

func checkAndResizeBuffer(buf []byte, need int) []byte {
	buf = buf[:cap(buf)]

	for cap(buf) < need {
		buf = append(buf, make([]byte, 2048)...)
	}

	return buf
}

package room

import (
	"errors"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	"sync"
	"time"

	"github.com/google/uuid"
)

type Room struct {
	mutex       *sync.RWMutex
	Termination chan struct{}

	name           string
	uuid           string
	connectedPeers map[uint32]string // internal room ID to transport uuid
	peerUuids      []string
	lastClientId   uint32

	currentConnectedClients int
	maxClientsSlots         int

	lastLeaveTime time.Time
	ttl           time.Duration
	tickrate      int
}

func NewRoom(name string, maxSlots int, ttl time.Duration) *Room {
	return &Room{
		mutex:                   &sync.RWMutex{},
		Termination:             make(chan struct{}),
		name:                    name,
		uuid:                    uuid.New().String(),
		connectedPeers:          make(map[uint32]string),
		peerUuids:               make([]string, 0),
		lastClientId:            0,
		currentConnectedClients: 0,
		maxClientsSlots:         maxSlots,
		lastLeaveTime:           time.Now().UTC(),
		ttl:                     ttl,
		tickrate:                30,
	}
}

func (r *Room) CreateTickPackets() (*datatransferobjects.GameTickPacket, *datatransferobjects.GameUnreliableTickPacket) {
	return nil, nil
}

func (r *Room) GetTickrate() int {
	return r.tickrate
}

func (r *Room) GetName() string {
	return r.name
}

func (r *Room) GetUuid() string {
	return r.uuid
}

func (r *Room) GetCurrentConnectedPeersCount() int32 {
	return int32(len(r.connectedPeers))
}

func (r *Room) GetAllConnectedPeers() []string {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.peerUuids
}

func (r *Room) GetMaxPlayersCount() int32 {
	return int32(r.maxClientsSlots)
}

func (r *Room) IsTimeFromLastLeaveOverTTL() bool {
	return time.Now().UTC().Sub(r.lastLeaveTime) > r.ttl
}

func (r *Room) AddPeer(uuid string) (int, error) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	if r.currentConnectedClients >= r.maxClientsSlots {
		return 0, errors.New("Max cleints exeeted")
	}

	newPeerId := r.lastClientId
	r.connectedPeers[newPeerId] = uuid
	r.lastClientId++

	r.peerUuids = make([]string, 0)

	for _, currentUuid := range r.connectedPeers {
		r.peerUuids = append(r.peerUuids, currentUuid)
	}

	return int(newPeerId), nil
}

func (r *Room) RemovePeer(uuid string) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	for key, currentUuid := range r.connectedPeers {
		if currentUuid == uuid {
			r.lastLeaveTime = time.Now().UTC()
			delete(r.connectedPeers, key)
			break
		}
	}

	r.peerUuids = make([]string, 0)

	for _, currentUuid := range r.connectedPeers {
		r.peerUuids = append(r.peerUuids, currentUuid)
	}
}

func (r *Room) GetTransportIdOfPeer(peer uint32) (string, error) {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	peerTransportUuid, hasPeer := r.connectedPeers[peer]

	if !hasPeer {
		return "", errors.New("Not finded peer")
	}

	return peerTransportUuid, nil
}

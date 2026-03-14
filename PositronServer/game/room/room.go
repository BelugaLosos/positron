package room

import (
	"errors"
	"sync"
	"time"

	"github.com/google/uuid"
)

type Room struct {
	mutex *sync.RWMutex

	name           string
	uuid           string
	connectedPeers map[uint32]string // internal room ID to transport uuid
	lastClientId   uint32

	currentConnectedClients int
	maxClientsSlots         int

	lastLeaveTime time.Time
	ttl           time.Duration
}

func NewRoom(name string, maxSlots int, ttl time.Duration) *Room {
	return &Room{
		mutex:                   &sync.RWMutex{},
		name:                    name,
		uuid:                    uuid.New().String(),
		connectedPeers:          make(map[uint32]string),
		lastClientId:            0,
		currentConnectedClients: 0,
		maxClientsSlots:         maxSlots,
		ttl:                     ttl,
	}
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

func (r *Room) GetMaxPlayersCount() int32 {
	return int32(r.maxClientsSlots)
}

func (r *Room) IsTimeFromLastLeaveOverTTL() bool {
	return time.Now().UTC().Sub(r.lastLeaveTime) > r.ttl
}

func (r *Room) AddPeer(uuid string) error {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	if r.currentConnectedClients >= r.maxClientsSlots {
		return errors.New("Max cleints exeeted")
	}

	r.connectedPeers[r.lastClientId] = uuid
	r.lastClientId++

	return nil
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

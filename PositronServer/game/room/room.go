package room

import (
	"errors"
	gameentities "positron/game/gameEntities"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	roommodels "positron/game/room/roomModels"
	"sync"
	"time"

	"github.com/google/uuid"
)

type Room struct {
	mutex       *sync.RWMutex
	Termination chan struct{}

	name string
	uuid string

	connectedPeers map[uint32]string // internal room ID to transport uuid
	peerUuids      []string
	hostIndex      uint32

	lastClientId uint32

	currentConnectedClients int
	maxClientsSlots         int

	lastLeaveTime time.Time
	ttl           time.Duration
	tickrate      int

	scene        uint32
	ExternalData []byte

	gameObjectsModel *roommodels.GameObjectsModel
	netValuesModel   *roommodels.NetValuesModel
	rpcsModel        *roommodels.RpcsModel
}

func NewRoom(name string, maxSlots int, ttl time.Duration, scene uint32, externalData []byte) *Room {
	return &Room{
		mutex:                   &sync.RWMutex{},
		Termination:             make(chan struct{}),
		name:                    name,
		uuid:                    uuid.New().String(),
		connectedPeers:          make(map[uint32]string),
		peerUuids:               make([]string, 0),
		hostIndex:               0,
		lastClientId:            0,
		currentConnectedClients: 0,
		maxClientsSlots:         maxSlots,
		lastLeaveTime:           time.Now().UTC(),
		ttl:                     ttl,
		tickrate:                30,
		scene:                   scene,
		ExternalData:            externalData,
		gameObjectsModel:        roommodels.NewGameObjectsModel(),
		netValuesModel:          roommodels.NewNetValuesModel(),
		rpcsModel:               roommodels.NewRpcsModel(),
	}
}

func (r *Room) CreateTickPackets() (*datatransferobjects.GameTickPacket, *datatransferobjects.GameUnreliableTickPacket) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	worldModAdd, worldModRemove, worldModTransfer := r.gameObjectsModel.GetModification()

	gameTick := datatransferobjects.NewTickPacket(
		r.hostIndex,
		0,
		worldModAdd,
		worldModRemove,
		worldModTransfer,
		r.netValuesModel.GetTempMod(),
		r.rpcsModel.GetCurrentCallBuffer(),
	)

	gamePositionsTick := datatransferobjects.NewGameUnreliableTickPacket(
		r.gameObjectsModel.GetPositionMod(),
		0,
	)

	return gameTick, gamePositionsTick
}

func (r *Room) ResetTempBuffers() {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.gameObjectsModel.ResetTempBuffers()
	r.netValuesModel.ResetTempBuffers()
	r.rpcsModel.ResetTempBuffers()
}

func (r *Room) ProcessTick(packet *datatransferobjects.GameTickPacket) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	for i := range packet.GetNewObjects() {
		r.gameObjectsModel.AddGameObject(packet.GetNewObjects()[i], packet.GetSourceClient())
	}

	for i := range packet.GetRemovedObjects() {
		removingObj := packet.GetRemovedObjects()[i]
		attemptor := packet.GetSourceClient()

		wasRemoved := r.gameObjectsModel.TryRemoveGameObject(removingObj, attemptor)

		if wasRemoved {
			r.netValuesModel.RemoveAllValuesFromObject(removingObj)
		}
	}

	for i := range packet.GetValueMod() {
		r.netValuesModel.AddOrModify(packet.GetValueMod()[i])
	}

	for i := range packet.GetRpcs() {
		r.rpcsModel.Call(packet.GetRpcs()[i])
	}
}

func (r *Room) ProcessUnreliableTick(packet *datatransferobjects.GameUnreliableTickPacket) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.gameObjectsModel.MoveGameObjects(packet)
}

func (r *Room) GetHost() uint32 {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.hostIndex
}

func (r *Room) GetTickrate() int {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.tickrate
}

func (r *Room) GetScene() uint32 {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.scene
}

func (r *Room) GetExternalData() []byte {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.ExternalData
}

func (r *Room) GetName() string {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.name
}

func (r *Room) GetUuid() string {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.uuid
}

func (r *Room) GetCurrentConnectedPeersCount() int32 {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return int32(len(r.connectedPeers))
}

func (r *Room) GetAllConnectedPeers() []string {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.peerUuids
}

func (r *Room) GetMaxPlayersCount() int32 {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return int32(r.maxClientsSlots)
}

func (r *Room) IsTimeFromLastLeaveOverTTL() bool {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return time.Now().UTC().Sub(r.lastLeaveTime) > r.ttl
}

func (r *Room) GetWorld() ([]*gameentities.GameObject, []*gameentities.NetValue, []*gameentities.RpcCall) {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return r.gameObjectsModel.GetGameObjects(), r.netValuesModel.GetValues(), r.rpcsModel.GetCachedRpcs()
}

func (r *Room) AddPeer(uuid string) (uint32, error) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	if r.currentConnectedClients >= r.maxClientsSlots {
		return 0, errors.New("Max cleints exeeted")
	}

	r.lastClientId++
	newPeerId := r.lastClientId
	r.connectedPeers[newPeerId] = uuid

	if len(r.connectedPeers) == 1 {
		r.hostIndex = newPeerId
	}

	r.peerUuids = make([]string, 0)

	for _, currentUuid := range r.connectedPeers {
		r.peerUuids = append(r.peerUuids, currentUuid)
	}

	return newPeerId, nil
}

func (r *Room) RemovePeer(uuid string) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	removedPeer := uint32(0)

	for key, currentUuid := range r.connectedPeers {
		if currentUuid == uuid {
			r.lastLeaveTime = time.Now().UTC()
			removedPeer = key
			delete(r.connectedPeers, key)
			break
		}
	}

	if removedPeer == r.hostIndex {
		for key := range r.connectedPeers {
			r.hostIndex = key
			break
		}
	}

	r.peerUuids = make([]string, 0)

	for _, currentUuid := range r.connectedPeers {
		r.peerUuids = append(r.peerUuids, currentUuid)
	}

	r.gameObjectsModel.TransferObjectsFromClientToHost(removedPeer, r.hostIndex)
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

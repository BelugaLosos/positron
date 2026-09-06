package room

import (
	"errors"
	"fmt"
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	roommodels "positron/game/room/roomModels"
	"strconv"
	"sync"
	"time"

	"github.com/google/uuid"
)

type Room struct {
	mutex       *sync.RWMutex
	Termination chan struct{}

	name                  string
	uuid                  string
	isUuidPostfixWasAdden bool

	connectedPeers map[uint32]string // internal room ID to transport uuid
	peerUuids      []string
	hostIndex      uint32

	lastClientId uint32

	maxClientsSlots int32

	lastLeaveTime time.Time
	ttl           time.Duration
	tickrate      int

	scene        uint32
	ExternalData []byte

	gameObjectsModel *roommodels.GameObjectsModel
	netValuesModel   *roommodels.NetValuesModel
	rpcsModel        *roommodels.RpcsModel
	clock            *RoomClock

	gameTickPointer    *datatransferobjects.GameTickPacket
	gameUnrTickPointer *datatransferobjects.GameUnreliableTickPacket

	Ticker *time.Ticker
}

func clamp(current int, min int, max int) int {
	if current < min {
		return min
	}

	if current > max {
		return max
	}

	return current
}

func NewRoom(name string, maxSlots int32, ttl time.Duration, scene uint32, tickrate uint32, externalData []byte, rtLimit, rtThrashold int, rtForceDisable bool) *Room {
	log.Printf("Created room with params N: %v P_CAP: %v TTL: %v SC: %v T_RATE: %v DATA_SEGMENT: %v RT_L: %v RT_Th: %v RT_FORCE_DISABLE: %v", name, maxSlots, ttl, scene, tickrate, externalData, rtLimit, rtThrashold, rtForceDisable)

	gameObjectsModel := roommodels.NewGameObjectsModel(rtLimit, rtThrashold, rtForceDisable)

	return &Room{
		mutex:              &sync.RWMutex{},
		Termination:        make(chan struct{}),
		name:               name,
		uuid:               uuid.New().String(),
		connectedPeers:     make(map[uint32]string),
		peerUuids:          make([]string, 0),
		hostIndex:          0,
		lastClientId:       0,
		maxClientsSlots:    maxSlots,
		lastLeaveTime:      time.Now().UTC(),
		ttl:                ttl,
		tickrate:           clamp(int(tickrate), 1, 256),
		scene:              scene,
		ExternalData:       externalData,
		gameObjectsModel:   gameObjectsModel,
		netValuesModel:     roommodels.NewNetValuesModel(gameObjectsModel),
		rpcsModel:          roommodels.NewRpcsModel(),
		clock:              NewRoomClock(tickrate),
		gameTickPointer:    &datatransferobjects.GameTickPacket{},
		gameUnrTickPointer: &datatransferobjects.GameUnreliableTickPacket{},
		Ticker:             time.NewTicker((1 * time.Second) / time.Duration(tickrate)),
	}
}

func (r *Room) AddPostfixToUuid(postfix int) {
	if r.isUuidPostfixWasAdden {
		return
	}

	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.uuid = r.uuid + "-" + strconv.Itoa(postfix)
	r.isUuidPostfixWasAdden = true
}

func (r *Room) RecordStartupTimeOnClock() {
	r.clock.RecordNewStartupTime()
}

func (r *Room) Lock() {
	r.mutex.Lock()
}

func (r *Room) Unlock() {
	r.mutex.Unlock()
}

func (r *Room) CreateTickPackets() (*datatransferobjects.GameTickPacket, *datatransferobjects.GameUnreliableTickPacket, []byte, []byte) {
	worldModAdd, worldModRemove, worldModTransfer := r.gameObjectsModel.GetModification()
	ticksSinceStartup := r.clock.GetTicksAmountSinceStartup()

	netValuesAddenMeta := r.netValuesModel.GetAdden()
	netValuesModifiedMeta := r.netValuesModel.GetModified()
	rpcsModMeta, rpcsModArena := r.rpcsModel.GetCurrentCallBuffer()

	r.gameTickPointer.ReassignTickPacketData(
		ticksSinceStartup,
		r.hostIndex,
		0,
		worldModAdd,
		worldModRemove,
		worldModTransfer,
		netValuesAddenMeta,
		netValuesModifiedMeta,
		rpcsModMeta,
	)

	r.gameObjectsModel.EvaluateStaticScore()
	r.gameObjectsModel.StickStaticsToMoveDelta()

	r.gameUnrTickPointer.ReassignUnreliableTickPacket(
		ticksSinceStartup,
		r.gameObjectsModel.GetPositionMod(),
		0,
	)

	return r.gameTickPointer, r.gameUnrTickPointer, r.netValuesModel.ReadLocalTransientArena(), rpcsModArena
}

func (r *Room) ResetTempBuffers() {
	r.gameObjectsModel.ResetTempBuffers()
	r.netValuesModel.ResetTempBuffers()
	r.rpcsModel.ResetTempBuffers()
}

func (r *Room) ProcessTick(packet *datatransferobjects.GameTickPacket, netValuesArena, rpcsArena []byte) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	peerUuid := packet.GetSourceClient()
	if _, hasKey := r.connectedPeers[peerUuid]; !hasKey {
		fmt.Printf("Peer '%v' does not in but send tick", peerUuid)
		return
	}

	r.netValuesModel.PutTransientDataIncoming(netValuesArena)
	r.rpcsModel.PutTransientDataIncoming(rpcsArena)

	for i := range packet.GetNewObjects() {
		r.gameObjectsModel.AddGameObject(packet.GetNewObjects()[i], packet.GetSourceClient())
	}

	for i := range packet.GetNewValues() {
		r.netValuesModel.AddValue(packet.GetNewValues()[i], packet.GetSourceClient(), r.hostIndex)
	}

	for i := range packet.GetModValues() {
		r.netValuesModel.ModifyValue(packet.GetModValues()[i], packet.GetSourceClient(), r.hostIndex)
	}

	addMod := r.gameObjectsModel.GetSpecificAddModification()

	for i := range packet.GetRpcs() {
		r.rpcsModel.Call(packet.GetRpcs()[i], addMod)
	}

	for i := range packet.GetRemovedObjects() {
		removingObj := packet.GetRemovedObjects()[i]
		attemptor := packet.GetSourceClient()

		wasRemoved := r.gameObjectsModel.TryRemoveGameObject(removingObj, attemptor, r.hostIndex)

		if wasRemoved {
			r.netValuesModel.RemoveAllValuesFromObject(removingObj)
			r.rpcsModel.SanetizeBufferedCalls(removingObj)
		}
	}

	r.gameObjectsModel.TransferObjectsOwnershipToTargetClient(packet.GetTranferedObjects(), packet.GetSourceClient())
}

func (r *Room) ProcessUnreliableTick(packet *datatransferobjects.GameUnreliableTickPacket) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.gameObjectsModel.MoveGameObjects(packet, r.hostIndex)
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

func (r *Room) GetWorld() ([]gameentities.GameObject, []gameentities.NetValue, []gameentities.RpcCall, []byte, []byte) {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	netValuesMeta, netValuesArena := r.netValuesModel.GetValues()
	rpcsMeta, rpcsArena := r.rpcsModel.GetCachedRpcs()

	return r.gameObjectsModel.GetGameObjects(), netValuesMeta, rpcsMeta, netValuesArena, rpcsArena
}

func (r *Room) AddPeer(uuid string) (uint32, error) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	if int64(len(r.connectedPeers)) >= int64(r.maxClientsSlots) {
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

package roommodels

import (
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	"positron/util"
	"sync"
)

type GameObjectsModel struct {
	mutex *sync.Mutex

	searchMap      map[uint32]*gameentities.GameObject
	searchPosCache map[uint32]*gameentities.Tranform

	gameObjectsStructuredCache []*gameentities.GameObject

	tempAdd         []*gameentities.GameObject
	tempRemove      []uint32
	tempTransfer    []uint32
	tempPositionMod []*gameentities.Tranform

	lastId                            uint32
	staticRetransmissionsPerTickLimit int
	staticScoreToRetransmitThrashold  int
	isRetransmissionForceDisabled     bool
}

const (
	POSITION_DELTA_TO_SYNC = 0.05
	ROTATION_DELTA_TO_SYNC = 1.0
)

func NewGameObjectsModel(rtLinmit, rtThrashold int, rtForceDisable bool) *GameObjectsModel {
	return &GameObjectsModel{
		mutex:                             &sync.Mutex{},
		searchMap:                         make(map[uint32]*gameentities.GameObject),
		searchPosCache:                    make(map[uint32]*gameentities.Tranform),
		gameObjectsStructuredCache:        make([]*gameentities.GameObject, 0),
		tempAdd:                           make([]*gameentities.GameObject, 0),
		tempRemove:                        make([]uint32, 0),
		tempTransfer:                      make([]uint32, 0),
		tempPositionMod:                   make([]*gameentities.Tranform, 0),
		lastId:                            0,
		staticRetransmissionsPerTickLimit: rtLinmit,
		staticScoreToRetransmitThrashold:  rtThrashold,
		isRetransmissionForceDisabled:     rtForceDisable,
	}
}

func (g *GameObjectsModel) GetGameObjects() []*gameentities.GameObject {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.updateStructuredCache()
	return g.gameObjectsStructuredCache
}

func (g *GameObjectsModel) GetModification() ([]*gameentities.GameObject, []uint32, []uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.tempAdd, g.tempRemove, g.tempTransfer
}

func (g *GameObjectsModel) GetSpecificAddModification() []*gameentities.GameObject {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.tempAdd
}

func (g *GameObjectsModel) GetPositionMod() []*gameentities.Tranform {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.tempPositionMod
}

func (g *GameObjectsModel) ResetTempBuffers() {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	clear(g.tempAdd)
	clear(g.tempRemove)
	clear(g.tempTransfer)
	clear(g.tempPositionMod)

	g.tempAdd = g.tempAdd[:0]
	g.tempRemove = g.tempRemove[:0]
	g.tempTransfer = g.tempTransfer[:0]
	g.tempPositionMod = g.tempPositionMod[:0]

	g.updateStructuredCache()
}

func (g *GameObjectsModel) MoveGameObjects(movingPacket *datatransferobjects.GameUnreliableTickPacket) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	delta := movingPacket.GetMovedObjects()
	source := movingPacket.GetSourceClient()

	for i := range delta {
		position := delta[i]

		if position == nil {
			continue
		}

		gameObject, exist := g.searchMap[position.GetObjectId()]
		localAllocatedTransform, tExist := g.searchPosCache[position.GetObjectId()]

		if !exist || gameObject == nil {
			continue
		}

		if !tExist || localAllocatedTransform == nil {
			log.Fatalf("CRITICAL Maps desync in game objects model!")
			continue
		}

		if gameObject.GetOwnerId() == source &&
			(util.PointsDistance(position.GetPosition(), gameObject.GetPosition()) > POSITION_DELTA_TO_SYNC ||
				util.RotationBetweenEulerAngles(position.GetRotation(), gameObject.GetRotation()) > ROTATION_DELTA_TO_SYNC) {

			gameObject.Move(position.GetPosition(), position.GetRotation())
			localAllocatedTransform.Move(position.GetPosition(), position.GetRotation())

			localAllocatedTransform.ResetStaticScore()
			g.tempPositionMod = append(g.tempPositionMod, localAllocatedTransform)
		}
	}
}

func (g *GameObjectsModel) EvaluateStaticScore() {
	if g.isRetransmissionForceDisabled {
		return
	}

	g.mutex.Lock()
	defer g.mutex.Unlock()

	for _, transform := range g.searchPosCache {
		transform.EvaluateStaticScore()
	}
}

func (g *GameObjectsModel) StickStaticsToMoveDelta() {
	if g.isRetransmissionForceDisabled {
		return
	}

	g.mutex.Lock()
	defer g.mutex.Unlock()

	stickedAmount := 0

	for _, transform := range g.searchPosCache {
		if transform.GetStaticScore() >= g.staticScoreToRetransmitThrashold {
			g.tempPositionMod = append(g.tempPositionMod, transform)
			transform.ResetStaticScore()

			stickedAmount++
		}

		if stickedAmount >= g.staticRetransmissionsPerTickLimit {
			break
		}
	}
}

func (g *GameObjectsModel) AddGameObject(gameObject *gameentities.GameObject, owner uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.lastId++
	gameObject.SetIdAndOnwer(g.lastId, owner)

	g.tempAdd = append(g.tempAdd, gameObject)

	g.searchMap[g.lastId] = gameObject
	g.searchPosCache[g.lastId] = gameentities.NewTransform(gameObject)
}

func (g *GameObjectsModel) TryRemoveGameObject(id uint32, attemptor uint32) bool {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	success := false

	object, exist := g.searchMap[id]

	if exist && object.GetOwnerId() == attemptor {
		g.tempRemove = append(g.tempRemove, id)
		delete(g.searchMap, id)
		delete(g.searchPosCache, id)

		success = true
	}

	return success
}

func (g *GameObjectsModel) TransferObjectsFromClientToHost(clientId uint32, actualHost uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.updateStructuredCache()

	for i := range g.gameObjectsStructuredCache {
		if g.gameObjectsStructuredCache[i].GetOwnerId() == clientId {
			g.gameObjectsStructuredCache[i].SetIdAndOnwer(g.gameObjectsStructuredCache[i].GetId(), actualHost)
			g.addToTempTransfer(g.gameObjectsStructuredCache[i], actualHost)
		}
	}
}

func (g *GameObjectsModel) TransferObjectsOwnershipToTargetClient(requestedTransfer []uint32, newOwner uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.updateStructuredCache()

	for i := range g.gameObjectsStructuredCache {
		obj := g.gameObjectsStructuredCache[i]

		for j := range requestedTransfer {
			reqId := requestedTransfer[j]

			if obj.GetId() == reqId {
				obj.SetIdAndOnwer(obj.GetId(), newOwner)
				g.addToTempTransfer(obj, newOwner)
			}
		}
	}
}

func (g *GameObjectsModel) addToTempTransfer(obj *gameentities.GameObject, newOwner uint32) {
	g.tempTransfer = append(g.tempTransfer, newOwner)
	g.tempTransfer = append(g.tempTransfer, obj.GetId())
}

func (g *GameObjectsModel) updateStructuredCache() {
	clear(g.gameObjectsStructuredCache)
	g.gameObjectsStructuredCache = g.gameObjectsStructuredCache[:0]

	for _, obj := range g.searchMap {
		g.gameObjectsStructuredCache = append(g.gameObjectsStructuredCache, obj)
	}
}

package roommodels

import (
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	"positron/util"
	"sync"
)

type GameObjectsModel struct {
	mutex                 *sync.Mutex
	defaultEmptyObject    gameentities.GameObject
	defaultEmptyTransform gameentities.Tranform

	worldCache              []gameentities.GameObject
	flatObjectsContainer    []gameentities.GameObject
	flatTransformsContainer []gameentities.Tranform

	freedIds           []uint32
	lastId             uint32
	currntObjectsCount int

	addCache      []gameentities.GameObject
	moveCache     []gameentities.Tranform
	removeCache   []uint32
	transferCache []uint32

	staticRetransmissionsPerTickLimit int
	staticScoreToRetransmitThrashold  int
	isRetransmissionForceDisabled     bool
}

type ReadOnlyGameObjectsModel interface {
	ThreadUnsafeGetObjectOwner(objectId uint32) (uint32, bool)
	ThreadUnsafeIsObjectExists(objectId uint32) bool
}

const (
	POSITION_DELTA_TO_SYNC = 0.05
	ROTATION_DELTA_TO_SYNC = 1.0
	ALLOCATION_CHUNK       = 128
)

func NewGameObjectsModel(rtLinmit, rtThrashold int, rtForceDisable bool) *GameObjectsModel {
	return &GameObjectsModel{
		mutex:                             &sync.Mutex{},
		defaultEmptyObject:                gameentities.GameObject{},
		defaultEmptyTransform:             gameentities.Tranform{},
		worldCache:                        make([]gameentities.GameObject, 32),
		flatObjectsContainer:              make([]gameentities.GameObject, 32),
		flatTransformsContainer:           make([]gameentities.Tranform, 32),
		freedIds:                          make([]uint32, 0, 32),
		lastId:                            0,
		currntObjectsCount:                0,
		addCache:                          make([]gameentities.GameObject, 0, 32),
		moveCache:                         make([]gameentities.Tranform, 0, 32),
		removeCache:                       make([]uint32, 0, 32),
		transferCache:                     make([]uint32, 0, 32),
		staticRetransmissionsPerTickLimit: rtLinmit,
		staticScoreToRetransmitThrashold:  rtThrashold,
		isRetransmissionForceDisabled:     rtForceDisable,
	}
}

func (g *GameObjectsModel) GetGameObjects() []gameentities.GameObject {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.buildWorldCache()
	return g.worldCache
}

func (g *GameObjectsModel) GetModification() ([]gameentities.GameObject, []uint32, []uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.addCache, g.removeCache, g.transferCache
}

func (g *GameObjectsModel) GetSpecificAddModification() []gameentities.GameObject {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.addCache
}

func (g *GameObjectsModel) GetPositionMod() []gameentities.Tranform {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.moveCache
}

func (g *GameObjectsModel) ResetTempBuffers() {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.addCache = g.addCache[:0]
	g.moveCache = g.moveCache[:0]
	g.removeCache = g.removeCache[:0]
	g.transferCache = g.transferCache[:0]
}

func (g *GameObjectsModel) MoveGameObjects(movingPacket *datatransferobjects.GameUnreliableTickPacket) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	delta := movingPacket.GetMovedObjects()
	source := movingPacket.GetSourceClient()

	for i := range delta {
		position := delta[i]

		if position.GetObjectId() == 0 {
			continue
		}

		gameObjectCopy, exist := g.getObjectById(position.GetObjectId())

		if !exist {
			continue
		}

		localTransformCopy := g.flatTransformsContainer[gameObjectCopy.GetId()]

		if gameObjectCopy.GetOwnerId() == source &&
			(util.PointsDistance(position.GetPosition(), gameObjectCopy.GetPosition()) > POSITION_DELTA_TO_SYNC ||
				util.RotationBetweenEulerAngles(position.GetRotation(), gameObjectCopy.GetRotation()) > ROTATION_DELTA_TO_SYNC) {

			gameObjectCopy.Move(position.GetPosition(), position.GetRotation())
			localTransformCopy.Move(position.GetPosition(), position.GetRotation())
			localTransformCopy.ResetStaticScore()

			g.moveCache = append(g.moveCache, localTransformCopy)

			g.flatObjectsContainer[gameObjectCopy.GetId()] = gameObjectCopy
			g.flatTransformsContainer[localTransformCopy.GetObjectId()] = localTransformCopy
		}
	}
}

func (g *GameObjectsModel) EvaluateStaticScore() {
	if g.isRetransmissionForceDisabled {
		return
	}

	g.mutex.Lock()
	defer g.mutex.Unlock()

	for i := range g.flatTransformsContainer {

		if g.flatTransformsContainer[i].GetObjectId() == 0 {
			continue
		}

		transform := g.flatTransformsContainer[i]
		transform.EvaluateStaticScore()
		g.flatTransformsContainer[i] = transform
	}
}

func (g *GameObjectsModel) StickStaticsToMoveDelta() {
	if g.isRetransmissionForceDisabled {
		return
	}

	g.mutex.Lock()
	defer g.mutex.Unlock()

	stickedAmount := 0

	for i := range g.flatTransformsContainer {
		transform := g.flatTransformsContainer[i]

		if transform.GetObjectId() == 0 {
			continue
		}

		if transform.GetStaticScore() >= g.staticScoreToRetransmitThrashold {
			transform.ResetStaticScore()
			g.flatTransformsContainer[i] = transform

			g.moveCache = append(g.moveCache, transform)

			stickedAmount++
		}

		if stickedAmount >= g.staticRetransmissionsPerTickLimit {
			break
		}
	}
}

func (g *GameObjectsModel) AddGameObject(gameObject gameentities.GameObject, owner uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	id := g.generateId()
	gameObject.SetIdAndOnwer(id, owner)
	g.allocateChunkIfNeed(id)

	g.flatObjectsContainer[id] = gameObject

	transform := gameentities.NewTransform(gameObject)
	transform.Move(gameObject.GetPosition(), gameObject.GetRotation())
	g.flatTransformsContainer[id] = transform

	g.addCache = append(g.addCache, gameObject)

	g.currntObjectsCount++
}

func (g *GameObjectsModel) TryRemoveGameObject(id uint32, attemptor uint32) bool {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	success := false

	object, exist := g.getObjectById(id)

	if exist && object.GetOwnerId() == attemptor {
		g.removeCache = append(g.removeCache, id)
		g.freedIds = append(g.freedIds, id)

		g.flatObjectsContainer[id] = g.defaultEmptyObject
		g.flatTransformsContainer[id] = g.defaultEmptyTransform

		success = true
		g.currntObjectsCount--
	}

	return success
}

func (g *GameObjectsModel) TransferObjectsFromClientToHost(clientId uint32, actualHost uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	for i := range g.flatObjectsContainer {
		if g.flatObjectsContainer[i].GetId() == 0 {
			continue
		}

		gameObject := g.flatObjectsContainer[i]

		if gameObject.GetOwnerId() == clientId {
			gameObject.SetIdAndOnwer(gameObject.GetId(), actualHost)
			g.addToTempTransfer(gameObject, actualHost)
		}
	}
}

func (g *GameObjectsModel) TransferObjectsOwnershipToTargetClient(requestedTransfer []uint32, newOwner uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	for i := range requestedTransfer {
		requestedObject := g.flatObjectsContainer[requestedTransfer[i]]

		if requestedObject.GetId() == 0 {
			continue
		}

		requestedObject.SetIdAndOnwer(requestedObject.GetId(), newOwner)
		g.addToTempTransfer(requestedObject, newOwner)
	}
}

func (g *GameObjectsModel) ThreadUnsafeGetObjectOwner(objectId uint32) (uint32, bool) {
	if int64(objectId) >= int64(len(g.flatObjectsContainer)) {
		return 0, false
	}

	obj := g.flatObjectsContainer[objectId]

	if obj.GetId() == 0 {
		return 0, false
	}

	return obj.GetOwnerId(), true
}

func (g *GameObjectsModel) ThreadUnsafeIsObjectExists(objectId uint32) bool {
	if int64(objectId) >= int64(len(g.flatObjectsContainer)) {
		return false
	}

	return g.flatObjectsContainer[objectId].GetId() != 0
}

func (g *GameObjectsModel) addToTempTransfer(obj gameentities.GameObject, newOwner uint32) {
	g.transferCache = append(g.transferCache, newOwner)
	g.transferCache = append(g.transferCache, obj.GetId())

	g.flatObjectsContainer[obj.GetId()] = obj
}

func (g *GameObjectsModel) generateId() uint32 {
	var id uint32

	if len(g.freedIds) != 0 {
		id = g.freedIds[len(g.freedIds)-1]
		g.freedIds = g.freedIds[:len(g.freedIds)-1]
	} else {
		g.lastId++
		id = g.lastId
	}

	return id
}

func (g *GameObjectsModel) allocateChunkIfNeed(id uint32) {
	if int64(id) >= int64(len(g.flatObjectsContainer)) {
		g.flatObjectsContainer = append(g.flatObjectsContainer, make([]gameentities.GameObject, ALLOCATION_CHUNK)...)
		g.flatTransformsContainer = append(g.flatTransformsContainer, make([]gameentities.Tranform, ALLOCATION_CHUNK)...)
	}
}

func (g *GameObjectsModel) getObjectById(id uint32) (gameentities.GameObject, bool) {
	if int64(id) >= int64(len(g.flatObjectsContainer)) {
		return g.defaultEmptyObject, false
	}

	if g.flatObjectsContainer[id].GetId() == 0 {
		return g.defaultEmptyObject, false
	}

	return g.flatObjectsContainer[id], true
}

func (g *GameObjectsModel) buildWorldCache() {
	clear(g.worldCache)
	g.worldCache = g.worldCache[:0]

	for i := range g.flatObjectsContainer {
		obj := g.flatObjectsContainer[i]

		if obj.GetId() != 0 {
			g.worldCache = append(g.worldCache, obj)
		}
	}
}

package roommodels

import (
	gameentities "positron/game/gameEntities"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	"positron/util"
	"sync"
)

type GameObjectsModel struct {
	mutex *sync.Mutex

	searchMap      map[uint32]*gameentities.GameObject
	searchPosCache map[uint32]*gameentities.Tranform

	gameObjects []*gameentities.GameObject

	tempAdd         []*gameentities.GameObject
	tempRemove      []uint32
	tempTransfer    []uint32
	tempPositionMod []*gameentities.Tranform

	lastId uint32
}

const POSITION_DELTA_TO_SYNC = 0.05

func NewGameObjectsModel() *GameObjectsModel {
	return &GameObjectsModel{
		mutex:           &sync.Mutex{},
		searchMap:       make(map[uint32]*gameentities.GameObject),
		searchPosCache:  make(map[uint32]*gameentities.Tranform),
		gameObjects:     make([]*gameentities.GameObject, 0),
		tempAdd:         make([]*gameentities.GameObject, 0),
		tempRemove:      make([]uint32, 0),
		tempTransfer:    make([]uint32, 0),
		tempPositionMod: make([]*gameentities.Tranform, 0),
		lastId:          0,
	}
}

func (g *GameObjectsModel) GetGameObjects() []*gameentities.GameObject {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.gameObjects
}

func (g *GameObjectsModel) GetModification() ([]*gameentities.GameObject, []uint32, []uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.tempAdd, g.tempRemove, g.tempTransfer
}

func (g *GameObjectsModel) GetPositionMod() []*gameentities.Tranform {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	return g.tempPositionMod
}

func (g *GameObjectsModel) ResetTempBuffers() {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.tempAdd = g.tempAdd[:0]
	g.tempRemove = g.tempRemove[:0]
	g.tempTransfer = g.tempTransfer[:0]
	g.tempPositionMod = g.tempPositionMod[:0]
}

func (g *GameObjectsModel) MoveGameObjects(movingPacket *datatransferobjects.GameUnreliableTickPacket) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	delta := movingPacket.GetMovedObjects()
	source := movingPacket.GetSourceClient()

	for i := range delta {
		position := delta[i]
		gameObject := g.searchMap[position.GetObjectId()]

		if gameObject.GetOwnerId() == source && util.PointsDistance(position.GetPosition(), gameObject.GetPosition()) > POSITION_DELTA_TO_SYNC {
			gameObject.Move(position.GetPosition(), position.GetRotation())
			position.Move(position.GetPosition(), position.GetRotation())

			g.tempPositionMod = append(g.tempPositionMod, position)
		}
	}
}

func (g *GameObjectsModel) AddGameObject(gameObject *gameentities.GameObject, owner uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.lastId++
	gameObject.SetIdAndOnwer(g.lastId, owner)

	g.gameObjects = append(g.gameObjects, gameObject)
	g.tempAdd = append(g.tempAdd, gameObject)

	g.searchMap[g.lastId] = gameObject
	g.searchPosCache[g.lastId] = gameentities.NewTransform(gameObject)
}

func (g *GameObjectsModel) TryRemoveGameObject(id uint32, attemptor uint32) bool {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	success := false

	for i := range g.gameObjects {
		gameObject := g.gameObjects[i]

		if gameObject.GetId() == id && gameObject.GetOwnerId() == attemptor {
			g.gameObjects[i] = g.gameObjects[0]
			g.gameObjects = g.gameObjects[1:]

			g.tempRemove = append(g.tempRemove, gameObject.GetId())
			delete(g.searchMap, gameObject.GetId())
			delete(g.searchPosCache, gameObject.GetId())

			success = true
		}
	}

	return success
}

func (g *GameObjectsModel) TransferObjectsFromClientToHost(clientId uint32, actualHost uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	for i := range g.gameObjects {
		if g.gameObjects[i].GetOwnerId() == clientId {
			g.gameObjects[i].SetIdAndOnwer(g.gameObjects[i].GetId(), actualHost)

			g.tempTransfer = append(g.tempTransfer, g.gameObjects[i].GetId())
		}
	}
}

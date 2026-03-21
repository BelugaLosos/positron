package roommodels

import (
	gameentities "positron/game/gameEntities"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	"sync"
)

type GameObjectsModel struct {
	mutex *sync.Mutex

	searchMap   map[uint32]*gameentities.GameObject
	gameObjects []*gameentities.GameObject

	tempAdd      []*gameentities.GameObject
	tempRemove   []uint32
	tempTransfer []uint32

	lastId uint32
}

func NewGameObjectsModel() *GameObjectsModel {
	return &GameObjectsModel{
		mutex:        &sync.Mutex{},
		searchMap:    make(map[uint32]*gameentities.GameObject),
		gameObjects:  make([]*gameentities.GameObject, 0),
		tempAdd:      make([]*gameentities.GameObject, 0),
		tempRemove:   make([]uint32, 0),
		tempTransfer: make([]uint32, 0),
		lastId:       0,
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

	return nil // NEED IMPELEMT
}

func (g *GameObjectsModel) ResetTempBuffers() {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.tempAdd = g.tempAdd[:0]
	g.tempRemove = g.tempRemove[:0]
	g.tempTransfer = g.tempTransfer[:0]
}

func (g *GameObjectsModel) MoveGameObjects(movingPacket *datatransferobjects.GameUnreliableTickPacket) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	// NEED IMPLEMENTATION
}

func (g *GameObjectsModel) AddGameObject(gameObject *gameentities.GameObject, owner uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	g.lastId++
	gameObject.SetIdAndOnwer(g.lastId, owner)

	g.gameObjects = append(g.gameObjects, gameObject)
	g.tempAdd = append(g.tempAdd, gameObject)

	g.searchMap[g.lastId] = gameObject
}

func (g *GameObjectsModel) RemoveGameObject(id uint32, attemptor uint32) {
	g.mutex.Lock()
	defer g.mutex.Unlock()

	for i := range g.gameObjects {
		gameObject := g.gameObjects[i]

		if gameObject.GetId() == id && gameObject.GetOwnerId() == attemptor {
			g.gameObjects[i] = nil
			g.gameObjects[i] = g.gameObjects[0]
			g.gameObjects[0] = nil
			g.gameObjects = g.gameObjects[1:]

			g.tempRemove = append(g.tempRemove, gameObject.GetId())
			delete(g.searchMap, gameObject.GetId())
		}
	}
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

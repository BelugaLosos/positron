package roommodels

import gameentities "positron/game/gameEntities"

type GameObjectsModel struct {
	gameObjects []*gameentities.GameObject
}

func NewGameObjectsModel() *GameObjectsModel {
	return &GameObjectsModel{
		gameObjects: make([]*gameentities.GameObject, 0),
	}
}

func (g *GameObjectsModel) GetGameObjects() []*gameentities.GameObject {
	return g.gameObjects
}

func (g *GameObjectsModel) ResetTempBuffers() {

}

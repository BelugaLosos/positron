package tests

import (
	"log"
	gameentities "positron/game/gameEntities"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestGetGameObjects(t *testing.T) {
	model := roommodels.NewGameObjectsModel()
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, *gameentities.NewVector(0, 0, 0), *gameentities.NewVector(0, 0, 0)), 1)
	objs := model.GetGameObjects()

	add, _, _ := model.GetModification()

	if len(objs) != 1 && len(add) != 1 {
		t.Error("not length matches")
	}

	model.ResetTempBuffers()
	add, _, _ = model.GetModification()

	if len(add) != 0 {
		t.Error("Reset fault")
	}
}

func TestModifications(t *testing.T) {
	model := roommodels.NewGameObjectsModel()
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, *gameentities.NewVector(0, 0, 0), *gameentities.NewVector(0, 0, 0)), 1) //1
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, *gameentities.NewVector(0, 1, 0), *gameentities.NewVector(0, 1, 0)), 1) //2
	model.TryRemoveGameObject(1, 1)
	model.TransferObjectsFromClientToHost(1, 0)

	add, remove, transfer := model.GetModification()

	if len(add) != 2 {
		t.Error("add len")
	} else if add[0].GetId() != 1 || add[1].GetId() != 2 || add[0].GetOwnerId() != 1 || add[1].GetOwnerId() != 0 {
		t.Error("data corruption ")

		for i := range add {
			log.Println(add[i].GetId(), add[i].GetOwnerId())
		}
	}

	if len(remove) != 1 {
		t.Error("remove len")
	} else if remove[0] != 1 {
		t.Error("removed wronmg object")
	}

	if len(transfer) != 1 {
		t.Error("transfer len")
	} else if transfer[0] != 2 {
		t.Error("Tranfered wrong object")
	} else if len(model.GetGameObjects()) != 1 || model.GetGameObjects()[0].GetId() != 2 || model.GetGameObjects()[0].GetOwnerId() != 0 || model.GetGameObjects()[0].GetCreationId() != 0 || model.GetGameObjects()[0].GetAssetIndex() != 0 {
		t.Error("Wrong content of model")
	}

	pos := model.GetGameObjects()[0].GetPosition()
	rot := model.GetGameObjects()[0].GetRotation()

	if pos.GetX() != 0 || pos.GetY() != 1 || pos.GetZ() != 0 || rot.GetX() != 0 || rot.GetY() != 1 || rot.GetZ() != 0 {
		t.Error("position corrupted")
	}
}

func TestCyclicMod(t *testing.T) {
	model := roommodels.NewGameObjectsModel()

	for range 10 {
		model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, *gameentities.NewVector(0, 0, 0), *gameentities.NewVector(0, 0, 0)), 1)

		add, _, _ := model.GetModification()

		if len(add) != 1 {
			t.Error("Reset fault")
		}

		model.ResetTempBuffers()
	}

	if len(model.GetGameObjects()) != 10 {
		t.Error("not all stored")
	}
}

func TestMove(t *testing.T) {
	model := roommodels.NewGameObjectsModel()
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, *gameentities.NewVector(0, 0, 0), *gameentities.NewVector(0, 0, 0)), 1)

	move := make([]*gameentities.Tranform, 0)
	move = append(move, gameentities.NewTransform(gameentities.NewGameObject(1, 1, 3, 3, *gameentities.NewVector(1, 1, 1), *gameentities.NewVector(1, 1, 1))))

	movePacket := datatransferobjects.NewGameUnreliableTickPacket(move, 1)
	model.MoveGameObjects(movePacket)

	mod := model.GetPositionMod()
	pos := mod[0].GetPosition()
	rot := mod[0].GetRotation()

	if mod[0].GetObjectId() != 1 || pos.GetX() != 1 || pos.GetY() != 1 || pos.GetZ() != 1 ||
		rot.GetX() != 1 || rot.GetY() != 1 || rot.GetZ() != 1 {
		t.Error("Ivalid pos")
	}

	model.ResetTempBuffers()

	move = make([]*gameentities.Tranform, 0)
	move = append(move, gameentities.NewTransform(gameentities.NewGameObject(1, 1, 3, 3, *gameentities.NewVector(1, 1, 1), *gameentities.NewVector(1, 1, 1))))

	movePacket = datatransferobjects.NewGameUnreliableTickPacket(move, 1)
	model.MoveGameObjects(movePacket)

	mod = model.GetPositionMod()

	if len(mod) != 0 {
		t.Error("Invalid reset and distance check")
	}

	model.ResetTempBuffers()

	move = make([]*gameentities.Tranform, 0)
	move = append(move, gameentities.NewTransform(gameentities.NewGameObject(1, 1, 3, 3, *gameentities.NewVector(1, 2, 1), *gameentities.NewVector(1, 2, 1))))

	movePacket = datatransferobjects.NewGameUnreliableTickPacket(move, 1)
	model.MoveGameObjects(movePacket)

	mod = model.GetPositionMod()

	if len(mod) != 1 {
		t.Error("Invalid reset and distance check")
	}
}

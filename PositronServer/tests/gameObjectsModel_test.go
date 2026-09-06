package tests

import (
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestGetGameObjects(t *testing.T) {
	model := roommodels.NewGameObjectsModel(50, 30, false)
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)
	objs := model.GetGameObjects()

	add, _, _ := model.GetModification()

	if len(objs) != 1 || len(add) != 1 {
		t.Errorf("not length matches %v %v", len(objs), len(add))
	}

	model.ResetTempBuffers()
	add, _, _ = model.GetModification()

	if len(add) != 0 {
		t.Error("Reset fault")
	}
}

func TestModifications(t *testing.T) {
	model := roommodels.NewGameObjectsModel(50, 30, false)
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1) //1
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 1, 0), gameentities.NewVector(0, 1, 0)), 1) //2

	persistent := model.GetGameObjects()

	if len(persistent) != 2 || persistent[0].GetId() != 1 || persistent[1].GetId() != 2 || persistent[0].GetOwnerId() != 1 || persistent[1].GetOwnerId() != 1 {
		t.Error("Persistance failed")

		for i := range persistent {
			log.Println(persistent[i].GetId(), persistent[i].GetOwnerId())
		}
	}

	model.TryRemoveGameObject(1, 1, 0)
	model.TransferObjectsFromClientToHost(1, 0)

	add, remove, transfer := model.GetModification()

	if len(add) != 2 {
		t.Errorf("add len %v", len(add))
	} else if add[0].GetId() != 1 || add[1].GetId() != 2 || add[0].GetOwnerId() != 1 || add[1].GetOwnerId() != 1 {
		t.Error("data corruption")

		for i := range add {
			log.Println(add[i].GetId(), add[i].GetOwnerId())
		}
	}

	if len(remove) != 1 {
		t.Error("remove len")
	} else if remove[0] != 1 {
		t.Error("removed wronmg object")
	}

	if len(transfer) != 2 {
		t.Error("transfer len")
	} else if transfer[0] != 0 || transfer[1] != 2 {
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

func TestRemoveWithHostAuthority(t *testing.T) {
	model := roommodels.NewGameObjectsModel(50, 30, false)
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1) //1
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 1, 0), gameentities.NewVector(0, 1, 0)), 1) //2
	model.ResetTempBuffers()

	wasRemoved := model.TryRemoveGameObject(2, 4, 4)
	_, r, _ := model.GetModification()

	if !wasRemoved || len(r) != 1 || r[0] != 2 {
		t.Error("Not enought authority!")
	}
}

func TestCyclicMod(t *testing.T) {
	model := roommodels.NewGameObjectsModel(50, 30, false)

	for range 10 {
		model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)

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
	doTestMove(t, 1, 0)
}

func TestMoveWithHostAuthority(t *testing.T) {
	doTestMove(t, 2, 2)
}

func doTestMove(t *testing.T, source, host uint32) {
	model := roommodels.NewGameObjectsModel(50, 30, false)
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)

	move := make([]gameentities.Tranform, 0)
	move = append(move, gameentities.NewTransform(gameentities.NewGameObject(1, 1, 3, 3, gameentities.NewVector(1, 1, 1), gameentities.NewVector(1, 1, 1))))

	movePacket := datatransferobjects.NewGameUnreliableTickPacket(0, move, source)
	model.MoveGameObjects(movePacket, host)

	mod := model.GetPositionMod()
	pos := mod[0].GetPosition()
	rot := mod[0].GetRotation()

	if mod[0].GetObjectId() != 1 || pos.GetX() != 1 || pos.GetY() != 1 || pos.GetZ() != 1 ||
		rot.GetX() != 1 || rot.GetY() != 1 || rot.GetZ() != 1 {
		t.Error("Ivalid pos")
	}

	model.ResetTempBuffers()

	move = make([]gameentities.Tranform, 0)
	move = append(move, gameentities.NewTransform(gameentities.NewGameObject(1, 1, 3, 3, gameentities.NewVector(1, 1, 1), gameentities.NewVector(1, 1, 1))))

	movePacket = datatransferobjects.NewGameUnreliableTickPacket(0, move, 1)
	model.MoveGameObjects(movePacket, 0)

	mod = model.GetPositionMod()

	if len(mod) != 0 {
		t.Error("Invalid reset and distance check")
	}

	model.ResetTempBuffers()

	move = make([]gameentities.Tranform, 0)
	move = append(move, gameentities.NewTransform(gameentities.NewGameObject(1, 1, 3, 3, gameentities.NewVector(1, 2, 1), gameentities.NewVector(1, 2, 1))))

	movePacket = datatransferobjects.NewGameUnreliableTickPacket(0, move, 1)
	model.MoveGameObjects(movePacket, 0)

	mod = model.GetPositionMod()

	if len(mod) != 1 {
		t.Error("Invalid reset and distance check")
	}
}

func TestGoReset(t *testing.T) {
	model := roommodels.NewGameObjectsModel(50, 30, false)
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)
	model.ResetTempBuffers()

	mod1, mod2, mod3 := model.GetModification()

	if len(mod1) != 0 || len(mod2) != 0 || len(mod3) != 0 {
		t.Error("Ineffient reset")
	}
}

func TestGoRegister(t *testing.T) {
	model := roommodels.NewGameObjectsModel(50, 30, false)
	model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)
	model.ResetTempBuffers()

	objs := model.GetGameObjects()

	if len(objs) != 1 || objs[0].GetCreationId() != 0 || objs[0].GetId() != 1 || objs[0].GetOwnerId() != 1 {
		t.Error("Obj not resigtred or data corrupted")
	}
}

func TestStaticsRetransmit(t *testing.T) {
	model := roommodels.NewGameObjectsModel(2, 5, false)

	for range 2 {
		model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)
	}

	for i := range 30 { // emulate 1 second
		model.EvaluateStaticScore()
		model.StickStaticsToMoveDelta()

		mod := model.GetPositionMod()
		expectedScore := i + 1

		if len(mod) != 2 && expectedScore%5 == 0 && expectedScore != 0 {
			t.Errorf("Static glue is broken %v tick %v", len(mod), expectedScore)
		}

		model.ResetTempBuffers()
	}
}

func TestRetransmissionDisable(t *testing.T) {
	model := roommodels.NewGameObjectsModel(2, 5, true)

	for range 2 {
		model.AddGameObject(gameentities.NewGameObject(0, 1, 0, 0, gameentities.NewVector(0, 0, 0), gameentities.NewVector(0, 0, 0)), 1)
	}

	for range 30 { // emulate 1 second
		model.EvaluateStaticScore()
		model.StickStaticsToMoveDelta()

		mod := model.GetPositionMod()

		if len(mod) != 0 {
			t.Error("Disability was ignored")
		}

		model.ResetTempBuffers()
	}
}

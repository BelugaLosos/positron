package tests

import (
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestCallNonBuffered(t *testing.T) {
	model := roommodels.NewRpcsModel()

	addMod := []gameentities.GameObject{gameentities.NewGameObject(0, 2, 3, 4, gameentities.NewVector(0, 1, 2), gameentities.NewVector(0, 1, 2))}

	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_ALL, 1, []byte("hello"), false), addMod)
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_OTHERS, 1, []byte("hello"), false), addMod)
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_TARGET, 1, []byte("hello"), false), addMod)

	mod := model.GetCurrentCallBuffer()
	buffered := model.GetCachedRpcs()

	if len(mod) != 3 {
		t.Error("Not registred call")
	} else if string(mod[0].GetArgs()) != "hello" || mod[0].GetMethodId() != 1 || mod[0].GetRpcType() != eventtypes.RPC_ALL || mod[0].GetTargetClient() != 1 ||
		mod[0].GetObjectId() != 0 || mod[0].GetSubObjectId() != 0 {
		t.Error("Data corrupted")
	}

	if len(buffered) != 0 {
		t.Error("Non buffered caprured into buffer")
	}
}

func TestResetCalls(t *testing.T) {
	model := roommodels.NewRpcsModel()

	addMod := []gameentities.GameObject{gameentities.NewGameObject(0, 2, 3, 4, gameentities.NewVector(0, 1, 2), gameentities.NewVector(0, 1, 2))}

	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_ALL, 0, []byte("hello"), false), addMod)
	model.ResetTempBuffers()
	mod := model.GetCurrentCallBuffer()

	if len(mod) != 0 {
		t.Error("Unefficient reset")
	}
}

func TestBufferedCall(t *testing.T) {
	model := roommodels.NewRpcsModel()

	addMod := []gameentities.GameObject{gameentities.NewGameObject(0, 2, 3, 4, gameentities.NewVector(0, 1, 2), gameentities.NewVector(0, 1, 2))}

	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_ALL_CACHED, 0, []byte("hello"), false), addMod)
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_OTHERS_CACHED, 0, []byte("hello"), false), addMod)
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_TARGET_CACHED, 0, []byte("hello"), false), addMod)
	model.ResetTempBuffers()
	buf := model.GetCachedRpcs()

	if len(buf) != 3 {
		t.Error("Non capruted buffered rpc")
	}
}

func TestRpcDto(t *testing.T) {
	rpc := gameentities.NewRpcCall(1, 2, 3, 4, 0, []byte{0x1, 0x12, 0x34, 0x22, 0x22}, true)

	if rpc.GetArgs()[0] != 0x22 || rpc.GetArgs()[1] != 0x22 || len(rpc.GetArgs()) != 2 {
		t.Errorf("corrupting of args %v", rpc.GetArgs())
	}

	if has, id := rpc.TryGetCreationId(); has != true || id != 0x1234 {
		t.Errorf("Corrupted the creation id %v %v", id, has)
	}

	rpc2 := gameentities.NewRpcCall(1, 2, 3, 4, 0, []byte{0x22, 0x22}, false)

	if rpc2.GetArgs()[0] != 0x22 || rpc2.GetArgs()[1] != 0x22 || len(rpc2.GetArgs()) != 2 {
		t.Errorf("corrupting of args %v", rpc2.GetArgs())
	}

	if has, id := rpc2.TryGetCreationId(); has != false || id != 0x0 {
		t.Errorf("Corrupted the creation id %v %v", id, has)
	}
}

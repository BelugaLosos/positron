package tests

import (
	"log"
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestCallNonBuffered(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte("XhelloXallghXolleh")   // hello allgh olleh WITH HEADERS
	expectedArenaOutput := []byte("helloallgholleh") // hello allgh olleh
	model.PutTransientDataIncoming(arenaImitation)

	log.Println(arenaImitation[0], arenaImitation[1])

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 6, 1, 4, 9, eventtypes.RPC_ALL, 5), addMod, 1, 0)
	model.Call(gameentities.NewRpcCall(6, 6, 2, 5, 10, eventtypes.RPC_OTHERS, 6), addMod, 1, 0)
	model.Call(gameentities.NewRpcCall(12, 6, 3, 6, 11, eventtypes.RPC_TARGET, 7), addMod, 1, 0)

	modMeta, modArena := model.GetCurrentCallBuffer()
	buffered, bufferedArena := model.GetCachedRpcs()

	if len(modMeta) != 3 ||
		modMeta[0] != gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL, 5) ||
		modMeta[1] != gameentities.NewRpcCall(5, 5, 2, 5, 10, eventtypes.RPC_OTHERS, 6) ||
		modMeta[2] != gameentities.NewRpcCall(10, 5, 3, 6, 11, eventtypes.RPC_TARGET, 7) {
		t.Errorf("Meta corruption %v %v", modMeta, len(modMeta))
	}

	if len(buffered) != 0 || len(bufferedArena) != 0 {
		t.Error("Cache for what ?")
	}

	if string(modArena) != string(expectedArenaOutput) {
		t.Errorf("arena corruption %s", string(modArena))
	}
}

func TestResetCalls(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte("helloallgholleh") // hello allgh olleh
	model.PutTransientDataIncoming(arenaImitation)

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL, 5), addMod, 1, 0)
	model.ResetTempBuffers()

	modMeta, modArena := model.GetCurrentCallBuffer()

	if len(modMeta) != 0 || len(modArena) != 0 {
		t.Error("Unefficient reset")
	}
}

func TestBufferedCall(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte("XhelloXallgh")    // hello allgh WITH HEADERS
	expectedArenaOutput := []byte("helloallgh") // hello allgh
	model.PutTransientDataIncoming(arenaImitation)

	log.Println(arenaImitation[0], arenaImitation[1])

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 6, 1, 4, 9, eventtypes.RPC_ALL_CACHED, 5), addMod, 1, 0)
	model.Call(gameentities.NewRpcCall(6, 6, 2, 5, 10, eventtypes.RPC_OTHERS_CACHED, 6), addMod, 1, 0)
	model.ResetTempBuffers()

	meta, arena := model.GetCachedRpcs()

	if len(meta) != 2 ||
		meta[0] != gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL_CACHED, 5) ||
		meta[1] != gameentities.NewRpcCall(5, 5, 2, 5, 10, eventtypes.RPC_OTHERS_CACHED, 6) {
		t.Error("Meta corruption")
	}

	if string(arena) != string(expectedArenaOutput) {
		t.Errorf("arena corruption %s", string(arena))
	}
}

func TestRpcDto(t *testing.T) {
	arena := []byte{0x1, 0x12, 0x34, 0x22, 0x22}

	if gameentities.GetRpcArgs(arena)[0] != 0x22 || gameentities.GetRpcArgs(arena)[1] != 0x22 || len(gameentities.GetRpcArgs(arena)) != 2 {
		t.Errorf("corrupting of args %v", gameentities.GetRpcArgs(arena))
	}

	if has, id, payload, success := gameentities.DeconstructRpc(arena); has != true || id != 0x1234 || len(payload) != 2 || payload[0] != 0x22 || payload[1] != 0x22 || success == false {
		t.Errorf("Corrupted the creation id %v %v", id, has)
	}

	arena = []byte{0x0, 0x22, 0x22}

	if gameentities.GetRpcArgs(arena)[0] != 0x22 || gameentities.GetRpcArgs(arena)[1] != 0x22 || len(gameentities.GetRpcArgs(arena)) != 2 {
		t.Errorf("corrupting of args %v", gameentities.GetRpcArgs(arena))
	}

	if has, id, payload, success := gameentities.DeconstructRpc(arena); has != false || id != 0x0 || len(payload) != 2 || payload[0] != 0x22 || payload[1] != 0x22 || success == false {
		t.Errorf("Corrupted the creation id %v %v %v", id, has, payload)
	}
}

func TestRpcInRemovedObjects(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte("Xhello")
	model.PutTransientDataIncoming(arenaImitation)
	model.Call(gameentities.NewRpcCall(0, 6, 1, 1, 0, eventtypes.RPC_ALL_CACHED, 0), nil, 1, 0)
	model.ResetTempBuffers()
	model.SanetizeBufferedCalls(1)
	meta, _ := model.GetCachedRpcs()

	if len(meta) != 1 || meta[0].GetRpcType() != eventtypes.RPC_INVALID {
		t.Errorf("not work: %v (len %v)", meta[0], len(meta))
	}
}

func TestRpcFromHost(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 1, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte("Xhello")
	expectedOutputArena := "hello"
	model.PutTransientDataIncoming(arenaImitation)

	model.Call(gameentities.NewRpcCall(0, 6, 1, 1, 0, eventtypes.RPC_ALL, 0), nil, 4, 0)

	meta, _ := model.GetCurrentCallBuffer()

	if len(meta) != 0 {
		t.Error("Call was successfull but not authorized")
	}

	model.ResetTempBuffers()

	model.Call(gameentities.NewRpcCall(0, 6, 1, 1, 0, eventtypes.RPC_ALL, 0), nil, 4, 4)

	meta, arena := model.GetCurrentCallBuffer()

	if len(meta) == 0 || string(arena) != expectedOutputArena {
		t.Errorf("Error with host call %v", len(meta))
	}
}

func TestRpcSanetizeCallBuffer(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	gom.AddGameObject(gameentities.NewGameObject(1, 1, 0, 5, gameentities.Vector3{}, gameentities.Vector3{}), 1)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte{0x1, 0x0, 0x5, 0x2, 0x3, 0x4}
	model.PutTransientDataIncoming(arenaImitation)

	adden, _, _ := gom.GetModification()
	model.Call(gameentities.NewRpcCall(0, 6, 0, 1, 0, eventtypes.RPC_ALL_CACHED, 0), adden, 4, 4)

	succes := gom.TryRemoveGameObject(1, 1, 4)

	if !succes {
		t.Error("deletion err")
	}

	model.SanetizeBufferedCalls(1)

	bcalls, resultiveArena := model.GetCachedRpcs()
	ccalls, cra := model.GetCurrentCallBuffer()

	if len(ccalls) != 1 {
		t.Error("uc")
	}

	if len(bcalls) != 1 {
		t.Error("uneffcient cached call")
	}

	if bcalls[0].GetRpcType() != eventtypes.RPC_INVALID || ccalls[0].GetRpcType() != eventtypes.RPC_ALL_CACHED {
		t.Error("Fake invalidisation")
	}

	if ptr, len := bcalls[0].GetDescriptors(); ptr != 0 || len != 3 {
		t.Error("payload corrupt")
	}

	if ptr, len := ccalls[0].GetDescriptors(); ptr != 0 || len != 3 {
		t.Error("payload corrupt")
	}

	if bcalls[0].GetObjectId() != 1 {
		t.Error("invalid resolving")
	}

	if ccalls[0].GetObjectId() != 1 {
		t.Error("invalid resolving")
	}

	if bcalls[0].GetTargetClient() != 1 {
		t.Error("target client corrupts")
	}

	if ccalls[0].GetTargetClient() != 1 {
		t.Error("target client corrupts")
	}

	if len(resultiveArena) != 3 || resultiveArena[0] != 0x2 || resultiveArena[1] != 0x3 || resultiveArena[2] != 0x4 {
		t.Error("arena is invalid")
	}

	if len(cra) != 3 || cra[0] != 0x2 || cra[1] != 0x3 || cra[2] != 0x4 {
		t.Error("arena is invalid")
	}
}

func TestCallRpcOnNonExistingObject(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	model := roommodels.NewRpcsModel(gom)
	arenaImitation := []byte("helloallgholleh") // hello allgh olleh
	model.PutTransientDataIncoming(arenaImitation)

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL, 5), addMod, 1, 0)

	cb, _ := model.GetCurrentCallBuffer()

	if len(cb) != 0 {
		t.Error("passed to nil object!")
	}
}

func TestUnsuccessfullFreshCall(t *testing.T) {
	gom := roommodels.NewGameObjectsModel(1, 1, true)
	model := roommodels.NewRpcsModel(gom)

	rpcIncomingArena := []byte{0x1, 0xFF, 0xFF, 0x3, 0x4, 0x5, 0x6}
	delta := []gameentities.GameObject{gameentities.NewGameObject(10, 1, 1, 1, gameentities.Vector3{}, gameentities.Vector3{})}

	model.PutTransientDataIncoming(rpcIncomingArena)
	model.Call(gameentities.NewRpcCall(0, 7, 0, 1, 0, eventtypes.RPC_ALL, 0), delta, 2, 2)

	calls, _ := model.GetCurrentCallBuffer()

	if len(calls) != 0 {
		t.Error("call was deriacted to non-existing obj but somehow got registred")
	}
}

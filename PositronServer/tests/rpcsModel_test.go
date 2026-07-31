package tests

import (
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestCallNonBuffered(t *testing.T) {
	model := roommodels.NewRpcsModel()
	arenaImitation := []byte("helloallgholleh") // hello allgh olleh
	model.PutTransientDataIncoming(arenaImitation)

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL, 5), addMod)
	model.Call(gameentities.NewRpcCall(5, 5, 2, 5, 10, eventtypes.RPC_OTHERS, 6), addMod)
	model.Call(gameentities.NewRpcCall(10, 5, 3, 6, 11, eventtypes.RPC_TARGET, 7), addMod)

	modMeta, modArena := model.GetCurrentCallBuffer()
	buffered, bufferedArena := model.GetCachedRpcs()

	if len(modMeta) != 3 ||
		modMeta[0] != gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL, 5) ||
		modMeta[1] != gameentities.NewRpcCall(5, 5, 2, 5, 10, eventtypes.RPC_OTHERS, 6) ||
		modMeta[2] != gameentities.NewRpcCall(10, 5, 3, 6, 11, eventtypes.RPC_TARGET, 7) {
		t.Error("Meta corruption")
	}

	if len(buffered) != 0 || len(bufferedArena) != 0 {
		t.Error("Cache for what ?")
	}

	if string(modArena) != string(arenaImitation) {
		t.Errorf("arena corruption %s", string(modArena))
	}
}

func TestResetCalls(t *testing.T) {
	model := roommodels.NewRpcsModel()
	arenaImitation := []byte("helloallgholleh") // hello allgh olleh
	model.PutTransientDataIncoming(arenaImitation)

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL, 5), addMod)
	model.ResetTempBuffers()

	modMeta, modArena := model.GetCurrentCallBuffer()

	if len(modMeta) != 0 || len(modArena) != 0 {
		t.Error("Unefficient reset")
	}
}

func TestBufferedCall(t *testing.T) {
	model := roommodels.NewRpcsModel()
	arenaImitation := []byte("helloallgholleh") // hello allgh olleh
	model.PutTransientDataIncoming(arenaImitation)

	addMod := []gameentities.GameObject{}

	model.Call(gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL_CACHED, 5), addMod)
	model.Call(gameentities.NewRpcCall(5, 5, 2, 5, 10, eventtypes.RPC_OTHERS_CACHED, 6), addMod)
	model.Call(gameentities.NewRpcCall(10, 5, 3, 6, 11, eventtypes.RPC_TARGET_CACHED, 7), addMod)
	model.ResetTempBuffers()

	meta, arena := model.GetCachedRpcs()

	if len(meta) != 3 ||
		meta[0] != gameentities.NewRpcCall(0, 5, 1, 4, 9, eventtypes.RPC_ALL_CACHED, 5) ||
		meta[1] != gameentities.NewRpcCall(5, 5, 2, 5, 10, eventtypes.RPC_OTHERS_CACHED, 6) ||
		meta[2] != gameentities.NewRpcCall(10, 5, 3, 6, 11, eventtypes.RPC_TARGET_CACHED, 7) {
		t.Error("Meta corruption")
	}

	if string(arena) != string(arenaImitation) {
		t.Errorf("arena corruption %s", string(arena))
	}
}

func TestRpcDto(t *testing.T) {
	arena := []byte{0x1, 0x12, 0x34, 0x22, 0x22}

	if gameentities.GetRpcArgs(arena)[0] != 0x22 || gameentities.GetRpcArgs(arena)[1] != 0x22 || len(gameentities.GetRpcArgs(arena)) != 2 {
		t.Errorf("corrupting of args %v", gameentities.GetRpcArgs(arena))
	}

	if has, id := gameentities.TryGetCreationIdRpc(arena); has != true || id != 0x1234 {
		t.Errorf("Corrupted the creation id %v %v", id, has)
	}

	arena = []byte{0x0, 0x22, 0x22}

	if gameentities.GetRpcArgs(arena)[0] != 0x22 || gameentities.GetRpcArgs(arena)[1] != 0x22 || len(gameentities.GetRpcArgs(arena)) != 2 {
		t.Errorf("corrupting of args %v", gameentities.GetRpcArgs(arena))
	}

	if has, id := gameentities.TryGetCreationIdRpc(arena); has != false || id != 0x0 {
		t.Errorf("Corrupted the creation id %v %v", id, has)
	}
}

package tests

import (
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestCallNonBuffered(t *testing.T) {
	model := roommodels.NewRpcsModel()
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_ALL, "test", []byte("hello")))
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_OTHERS, "test", []byte("hello")))
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_TARGET, "test", []byte("hello")))

	mod := model.GetCurrentCallBuffer()
	buffered := model.GetCachedRpcs()

	if len(mod) != 3 {
		t.Error("Not registred call")
	} else if string(mod[0].GetArgs()) != "hello" || mod[0].GetMethodName() != "test" || mod[0].GetTarget() != eventtypes.RPC_ALL || mod[0].GetTargetClient() != 1 ||
		mod[0].GetObjectId() != 0 || mod[0].GetSubObjectId() != 0 {
		t.Error("Data corrupted")
	}

	if len(buffered) != 0 {
		t.Error("Non buffered caprured into buffer")
	}
}

func TestResetCalls(t *testing.T) {
	model := roommodels.NewRpcsModel()
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_ALL, "test", []byte("hello")))
	model.ResetTempBuffers()
	mod := model.GetCurrentCallBuffer()

	if len(mod) != 0 {
		t.Error("Unefficient reset")
	}
}

func TestBufferedCall(t *testing.T) {
	model := roommodels.NewRpcsModel()
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_ALL_CACHED, "test", []byte("hello")))
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_OTHERS_CACHED, "test", []byte("hello")))
	model.Call(gameentities.NewRpcCall(0, 1, 0, eventtypes.RPC_TARGET_CACHED, "test", []byte("hello")))
	model.ResetTempBuffers()
	buf := model.GetCachedRpcs()

	if len(buf) != 3 {
		t.Error("Non capruted buffered rpc")
	}
}

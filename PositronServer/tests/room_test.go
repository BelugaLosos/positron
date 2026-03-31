package tests

import (
	gameentities "positron/game/gameEntities"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	"positron/game/room"
	"testing"
	"time"
)

func TestRoomGetters(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second)

	if room.GetHost() != 0 {
		t.Error("host corruption")
	}

	if room.GetMaxPlayersCount() != 10 {
		t.Error("Player cap corruption")
	}

	if room.GetName() != "test" {
		t.Error("Name corruption")
	}

	if room.GetTickrate() != 30 {
		t.Error("Wrong tickrate")
	}

	if room.GetUuid() == "" {
		t.Error("uuid is empty")
	}
}

func TestAddPeer(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second)

	id, err := room.AddPeer("1")

	if id != 1 {
		t.Error("wrong id")
	}

	if err != nil {
		t.Error(err)
	}

	id, err = room.AddPeer("2")

	if id != 2 {
		t.Error("wrong id")
	}

	if err != nil {
		t.Error(err)
	}

	peers := room.GetAllConnectedPeers()

	if len(peers) != 2 || peers[0] != "1" || peers[1] != "2" {
		t.Errorf("not registred peers %v len %v", peers, len(peers))
	}
}

func TestRemovePeer(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second)
	room.AddPeer("1")
	room.AddPeer("2")

	room.RemovePeer("2")

	if len(room.GetAllConnectedPeers()) != 1 {
		t.Error("unefficient remove")
	}
}

func TestTick(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second)
	room.AddPeer("1111")
	room.ProcessTick(datatransferobjects.NewTickPacket(
		1,
		1,
		[]*gameentities.GameObject{gameentities.NewGameObject(0, 1, 1, 1, *gameentities.NewVector(1, 1, 1), *gameentities.NewVector(1, 1, 1))},
		[]uint32{},
		[]uint32{},
		[]*gameentities.NetValue{},
		[]*gameentities.RpcCall{},
	))

	rel, urel := room.CreateTickPackets()

	if rel == nil || urel == nil {
		t.Error("Packet is nil")
	}

	newGos := rel.GetNewObjects()

	if len(newGos) != 1 {
		t.Error("not valid len")
	} else if newGos[0].GetId() != 1 || newGos[0].GetCreationId() != 1 || newGos[0].GetAssetIndex() != 1 {
		t.Error("data corrupt")
	}

	room.ResetTempBuffers()

	rel, urel = room.CreateTickPackets()
	newGos = rel.GetNewObjects()

	if len(newGos) != 0 {
		t.Error("Unefficient reset")
	}

	worldObjects, worldValues, cachedRpcs := room.GetWorld()

	if len(worldObjects) != 1 || len(worldValues) != 0 || len(cachedRpcs) != 0 {
		t.Error("World corruption")
	} else if worldObjects[0].GetId() != 1 || worldObjects[0].GetCreationId() != 1 || worldObjects[0].GetAssetIndex() != 1 {
		t.Error("World data corrupt")
	}
}

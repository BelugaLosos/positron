package tests

import (
	"bytes"
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	"positron/game/room"
	"positron/internal/marshaller"
	"sync"
	"testing"
	"time"
)

func TestRoomGetters(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3))

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

	if room.GetScene() != 0 {
		t.Error("Wrong scene index")
	}

	if len(room.GetExternalData()) != 3 {
		t.Error("External data corrupt")
	}
}

func TestAddPeer(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3))

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

	if len(peers) != 2 || (peers[0] != "1" && peers[0] != "2") || (peers[1] != "2" && peers[1] != "1") { // peers is map and NOT strict organized!
		t.Errorf("not registred peers %v len %v", peers, len(peers))
	}
}

func TestRemovePeer(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3))
	room.AddPeer("1")
	room.AddPeer("2")

	room.RemovePeer("2")

	if len(room.GetAllConnectedPeers()) != 1 {
		t.Error("unefficient remove")
	}
}

func TestTick(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3))
	room.AddPeer("1111")
	room.ProcessTick(datatransferobjects.NewTickPacket(
		1,
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

func TestRaceInTick(t *testing.T) {
	m := marshaller.NewMessagePackMarshaller()
	r := room.NewRoom("t", 64, time.Hour, 0, 60, nil)

	cid, err := r.AddPeer("peer-uuid")
	if err != nil {
		log.Fatal(err)
	}

	stop := make(chan struct{})
	wg := &sync.WaitGroup{}

	for w := 0; w < 8; w++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			for {
				select {
				case <-stop:
					return
				default:
					pkt := datatransferobjects.NewTickPacket(
						1,
						0, cid,
						[]*gameentities.GameObject{gameentities.NewGameObject(0, cid, 1, 1, gameentities.Vector3{}, gameentities.Vector3{})},
						nil, nil,
						[]*gameentities.NetValue{{ParentObjectId: 1, Payload: []byte{1, 2, 3}}},
						[]*gameentities.RpcCall{gameentities.NewRpcCall(1, 0, 0, 0, "m", []byte{1})},
					)
					r.ProcessTick(pkt)
				}
			}
		}()
	}

	wg.Add(1)
	go func() {
		defer wg.Done()
		buf := &bytes.Buffer{}
		ubuf := &bytes.Buffer{}
		for {
			select {
			case <-stop:
				return
			default:
				r.Lock()

				packet, unrel := r.CreateTickPackets()
				_ = m.MarshalNonAlloc(buf, packet)
				_ = m.MarshalNonAlloc(ubuf, unrel)
				r.ReleaseTickPackets(packet, unrel)
				r.ResetTempBuffers()

				r.Unlock()
			}
		}
	}()

	time.Sleep(800 * time.Millisecond)
	close(stop)
	wg.Wait()
	log.Println("done, no panic")
}

func TestRoomTicker(t *testing.T) {
	passTickrate(t, 30)
	passTickrate(t, 60)
	passTickrate(t, 128)
}

func passTickrate(t *testing.T, tickrate uint32) {
	room := room.NewRoom("", 1, time.Hour, 1, tickrate, nil)
	ticked := 0
	wg := &sync.WaitGroup{}
	shutdown := make(chan struct{})

	wg.Add(1)
	go func() {
		defer wg.Done()

		for {
			select {
			case <-shutdown:
				return
			case <-room.Ticker.C:
				ticked++
			}
		}
	}()

	time.Sleep(1001 * time.Millisecond)
	close(shutdown)

	if ticked != int(tickrate) {
		t.Errorf("Inaccurate ticks! %v(EXPECTED) != %v(REAL TICKED BY SECOND)", tickrate, ticked)
	}
}

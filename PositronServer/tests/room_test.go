package tests

import (
	"bytes"
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	gamehandlers "positron/game/gameHandlers"
	"positron/game/room"
	"positron/internal"
	"positron/internal/marshaller"
	"sync"
	"testing"
	"time"
)

type mockGameServerForZeroAllocTest struct {
	mrsh *marshaller.MessagePackMarshaller
}

func (m *mockGameServerForZeroAllocTest) GetRoom(roomUuid string) *room.Room {
	return nil
}
func (m *mockGameServerForZeroAllocTest) GetAllRooms() []*room.Room {
	return nil
}
func (m *mockGameServerForZeroAllocTest) CreateRoom(name string, maxSlots int32, ttl time.Duration, scene uint32, tickrate uint32, externalData []byte) string {
	return "nil"
}
func (m *mockGameServerForZeroAllocTest) GetMarshaller() internal.MarshalService {
	return m.mrsh
}
func (m *mockGameServerForZeroAllocTest) GetVersion() string {
	return "nil"
}

func TestRoomGetters(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3), 50, 30, false)

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
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3), 50, 30, false)

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
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3), 50, 30, false)
	room.AddPeer("1")
	room.AddPeer("2")

	room.RemovePeer("2")

	if len(room.GetAllConnectedPeers()) != 1 {
		t.Error("unefficient remove")
	}
}

func TestTick(t *testing.T) {
	room := room.NewRoom("test", 10, 10*time.Second, 0, 30, make([]byte, 3), 50, 30, false)
	room.AddPeer("1111")
	room.ProcessTick(datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{gameentities.NewGameObject(0, 1, 1, 1, gameentities.NewVector(1, 1, 1), gameentities.NewVector(1, 1, 1))},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	), make([]byte, 0), make([]byte, 0))

	rel, urel, _, _ := room.CreateTickPackets()

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

	rel, urel, _, _ = room.CreateTickPackets()
	newGos = rel.GetNewObjects()

	if len(newGos) != 0 {
		t.Error("Unefficient reset")
	}

	worldObjects, worldValues, cachedRpcs, _, _ := room.GetWorld()

	if len(worldObjects) != 1 || len(worldValues) != 0 || len(cachedRpcs) != 0 {
		t.Error("World corruption")
	} else if worldObjects[0].GetId() != 1 || worldObjects[0].GetCreationId() != 1 || worldObjects[0].GetAssetIndex() != 1 {
		t.Error("World data corrupt")
	}
}

func TestRaceInTick(t *testing.T) {
	m := marshaller.NewMessagePackMarshaller()
	r := room.NewRoom("t", 64, time.Hour, 0, 60, nil, 50, 30, false)

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
						[]gameentities.GameObject{gameentities.NewGameObject(0, cid, 1, 1, gameentities.Vector3{}, gameentities.Vector3{})},
						nil, nil,
						[]gameentities.NetValue{},
						[]gameentities.PersistentNetValue{},
						[]gameentities.RpcCall{},
					)
					r.ProcessTick(pkt, make([]byte, 0), make([]byte, 0))
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

				packet, unrel, _, _ := r.CreateTickPackets()
				_ = m.MarshalNonAlloc(buf, packet)
				_ = m.MarshalNonAlloc(ubuf, unrel)

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
	room := room.NewRoom("", 1, time.Hour, 1, tickrate, nil, 50, 30, false)
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

func BenchmarkRoomTickColdPath(b *testing.B) {
	srv := &mockGameServerForZeroAllocTest{
		mrsh: marshaller.NewMessagePackMarshaller(),
	}

	room := room.NewRoom("room", 1, time.Hour, 1, 30, nil, 50, 30, true) // RT is bottleneck of entire server!
	handler := gamehandlers.NewGameTickHandler()
	handler.Init(nil, srv, "")
	handler.SetRoom(room, 1)
	room.AddPeer("")

	//this data is fully authentic tick snaphsot that requests creation of a single entity (cube gameObject) and passes 16 bytes of arena to arenas
	dataMock := []byte{} // TODO: replace with new one

	log.Println(b.N)

	b.ResetTimer()
	b.ReportAllocs()

	for i := 0; i < b.N; i++ {
		handler.PassHandle(dataMock) // this shit must come from net

		room.CreateTickPackets() // do shit

		room.ResetTempBuffers() // reset shit

		//this test does NOT mocks network
	}
}

func BenchmarkRoomTickColdPathWithoutUnmarshall(b *testing.B) {
	srv := &mockGameServerForZeroAllocTest{
		mrsh: marshaller.NewMessagePackMarshaller(),
	}

	room := room.NewRoom("room", 1, time.Hour, 1, 30, nil, 50, 30, true) // RT is bottleneck of entire server!
	handler := gamehandlers.NewGameTickHandler()
	handler.Init(nil, srv, "")
	handler.SetRoom(room, 1)
	room.AddPeer("")

	log.Println(b.N)

	tickPacket := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{gameentities.NewGameObject(0, 1, 1, 1, gameentities.Vector3{}, gameentities.Vector3{})},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	)

	tickPacketDestroy := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{},
		[]uint32{1},
		[]uint32{},
		[]gameentities.NetValue{},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	)

	emptyArena := make([]byte, 0)

	b.ResetTimer()
	b.ReportAllocs()

	for i := 0; i < b.N; i++ {
		room.ProcessTick(tickPacket, emptyArena, emptyArena)

		room.CreateTickPackets()
		room.ResetTempBuffers()

		room.ProcessTick(tickPacketDestroy, emptyArena, emptyArena)

		room.ResetTempBuffers()
	}
}

func BenchmarkRoomTickValuesColdPath(b *testing.B) {
	srv := &mockGameServerForZeroAllocTest{
		mrsh: marshaller.NewMessagePackMarshaller(),
	}

	room := room.NewRoom("room", 1, time.Hour, 1, 30, nil, 50, 30, false)
	handler := gamehandlers.NewGameTickHandler()
	handler.Init(nil, srv, "")
	handler.SetRoom(room, 1)
	room.AddPeer("")

	log.Println(b.N)

	tickPacketAddObj := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{gameentities.NewGameObject(1, 1, 1, 1, gameentities.Vector3{}, gameentities.Vector3{})},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	)

	valArena := []byte{0, 0, 0, 1}

	tickPacket := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{gameentities.NewNetValueAsTransient(0, 4, 1, 0, false)},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	)

	room.ProcessTick(tickPacketAddObj, valArena, valArena)

	b.ResetTimer()
	b.ReportAllocs()

	for i := 0; i < b.N; i++ {
		room.ProcessTick(tickPacket, valArena, valArena)

		room.CreateTickPackets()
		room.ResetTempBuffers()
	}
}

func BenchmarkRoomTickValuesHotPath(b *testing.B) {
	srv := &mockGameServerForZeroAllocTest{
		mrsh: marshaller.NewMessagePackMarshaller(),
	}

	room := room.NewRoom("room", 1, time.Hour, 1, 30, nil, 50, 30, false)
	handler := gamehandlers.NewGameTickHandler()
	handler.Init(nil, srv, "")
	handler.SetRoom(room, 1)
	room.AddPeer("")

	log.Println(b.N)

	tickPacketAddObj := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{gameentities.NewGameObject(1, 1, 1, 1, gameentities.Vector3{}, gameentities.Vector3{})},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	)

	valArena := []byte{0, 0, 0, 1}
	valArenaNew := []byte{0, 0, 2}

	tickPacketAddValue := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{gameentities.NewNetValueAsTransient(0, 4, 1, 0, false)},
		[]gameentities.PersistentNetValue{},
		[]gameentities.RpcCall{},
	)

	room.ProcessTick(tickPacketAddObj, valArena, valArena)
	room.ProcessTick(tickPacketAddValue, valArena, valArena)

	tickPacket := datatransferobjects.NewTickPacket(
		1,
		1,
		1,
		[]gameentities.GameObject{},
		[]uint32{},
		[]uint32{},
		[]gameentities.NetValue{},
		[]gameentities.PersistentNetValue{gameentities.NewPersistentNetValue(1, 0, 3, false)},
		[]gameentities.RpcCall{},
	)

	b.ResetTimer()
	b.ReportAllocs()

	for i := 0; i < b.N; i++ {
		room.ProcessTick(tickPacket, valArenaNew, valArenaNew)

		room.CreateTickPackets()
		room.ResetTempBuffers()
	}
}

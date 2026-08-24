package tests

import (
	"bytes"
	"log"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	"positron/internal/marshaller"
	"sync"
	"testing"

	"github.com/pierrec/lz4/v4"
)

func TestUnmarshalling(t *testing.T) {
	mrsh := marshaller.NewMessagePackMarshaller()

	for range 1_000_000 {
		obj := gameentities.NewGameObject(1, 2, 3, 4, gameentities.NewVector(5, 6, 7), gameentities.NewVector(8, 9, 10))
		val := gameentities.NewNetValueAsTransient(22, 33, 111, 123, true)
		valPersistant := gameentities.NewPersistentNetValue(1241, 666, 22, true)
		val.SetPersistentMemoryDescriptor(1)

		rpc := gameentities.NewRpcCall(1, 2, 11, 12, 13, 14, 1)

		testData := datatransferobjects.NewTickPacket(1, 15, 16, []gameentities.GameObject{obj}, []uint32{17}, []uint32{18}, []gameentities.NetValue{val}, []gameentities.PersistentNetValue{valPersistant}, []gameentities.RpcCall{rpc})

		buf := &bytes.Buffer{}
		err := mrsh.MarshalNonAlloc(buf, testData)
		marshalled := buf.Bytes()

		if err != nil {
			t.Error(err)
		}

		var unmarshalled datatransferobjects.GameTickPacket
		err = mrsh.Unmarshal(marshalled, &unmarshalled)

		if err != nil {
			t.Error(err)
		}

		tptr, tlen := testData.GetNewValues()[0].GetTransientMemoryDescriptor()
		uptr, ulen := unmarshalled.GetNewValues()[0].GetTransientMemoryDescriptor()

		rtptr, rtlen := testData.GetRpcs()[0].GetDescriptors()
		ruptr, rulen := unmarshalled.GetRpcs()[0].GetDescriptors()

		if testData.GetTick() != unmarshalled.GetTick() {
			t.Errorf("Data corrupt: Tick mismatch (expected %v, got %v)", testData.GetTick(), unmarshalled.GetTick())
		}
		if testData.GetHost() != unmarshalled.GetHost() {
			t.Errorf("Data corrupt: Host mismatch (expected %v, got %v)", testData.GetHost(), unmarshalled.GetHost())
		}
		if testData.GetSourceClient() != unmarshalled.GetSourceClient() {
			t.Errorf("Data corrupt: SourceClient mismatch (expected %v, got %v)", testData.GetSourceClient(), unmarshalled.GetSourceClient())
		}

		if len(testData.GetNewObjects()) != len(unmarshalled.GetNewObjects()) {
			t.Errorf("Data corrupt: NewObjects length mismatch (expected %d, got %d)", len(testData.GetNewObjects()), len(unmarshalled.GetNewObjects()))
		} else if len(testData.GetNewObjects()) > 0 {
			obj1 := testData.GetNewObjects()[0]
			obj2 := unmarshalled.GetNewObjects()[0]

			if obj1.GetId() != obj2.GetId() {
				t.Errorf("Data corrupt: NewObjects[0].Id mismatch (expected %v, got %v)",
					obj1.GetId(), obj2.GetId())
			}
			if obj1.GetOwnerId() != obj2.GetOwnerId() {
				t.Errorf("Data corrupt: NewObjects[0].OwnerId mismatch (expected %v, got %v)",
					obj1.GetOwnerId(), obj2.GetOwnerId())
			}
			if obj1.GetAssetIndex() != obj2.GetAssetIndex() {
				t.Errorf("Data corrupt: NewObjects[0].AssetIndex mismatch (expected %v, got %v)",
					obj1.GetAssetIndex(), obj2.GetAssetIndex())
			}
			if obj1.GetCreationId() != obj2.GetCreationId() {
				t.Errorf("Data corrupt: NewObjects[0].CreationId mismatch (expected %v, got %v)",
					obj1.GetCreationId(), obj2.GetCreationId())
			}

			if obj1.GetPosition().GetX() != obj2.GetPosition().GetX() {
				t.Errorf("Data corrupt: NewObjects[0].Position.X mismatch (expected %v, got %v)",
					obj1.GetPosition().GetX(), obj2.GetPosition().GetX())
			}
			if obj1.GetPosition().GetY() != obj2.GetPosition().GetY() {
				t.Errorf("Data corrupt: NewObjects[0].Position.Y mismatch (expected %v, got %v)",
					obj1.GetPosition().GetY(), obj2.GetPosition().GetY())
			}
			if obj1.GetPosition().GetZ() != obj2.GetPosition().GetZ() {
				t.Errorf("Data corrupt: NewObjects[0].Position.Z mismatch (expected %v, got %v)",
					obj1.GetPosition().GetZ(), obj2.GetPosition().GetZ())
			}

			if obj1.GetRotation().GetX() != obj2.GetRotation().GetX() {
				t.Errorf("Data corrupt: NewObjects[0].Rotation.X mismatch (expected %v, got %v)",
					obj1.GetRotation().GetX(), obj2.GetRotation().GetX())
			}
			if obj1.GetRotation().GetY() != obj2.GetRotation().GetY() {
				t.Errorf("Data corrupt: NewObjects[0].Rotation.Y mismatch (expected %v, got %v)",
					obj1.GetRotation().GetY(), obj2.GetRotation().GetY())
			}
			if obj1.GetRotation().GetZ() != obj2.GetRotation().GetZ() {
				t.Errorf("Data corrupt: NewObjects[0].Rotation.Z mismatch (expected %v, got %v)",
					obj1.GetRotation().GetZ(), obj2.GetRotation().GetZ())
			}
		}

		if len(testData.GetRemovedObjects()) != len(unmarshalled.GetRemovedObjects()) {
			t.Errorf("Data corrupt: RemovedObjects length mismatch (expected %d, got %d)", len(testData.GetRemovedObjects()), len(unmarshalled.GetRemovedObjects()))
		} else if len(testData.GetRemovedObjects()) > 0 {
			if testData.GetRemovedObjects()[0] != unmarshalled.GetRemovedObjects()[0] {
				t.Errorf("Data corrupt: RemovedObjects[0] mismatch (expected %v, got %v)", testData.GetRemovedObjects()[0], unmarshalled.GetRemovedObjects()[0])
			}
		}

		if len(testData.GetTranferedObjects()) != len(unmarshalled.GetTranferedObjects()) {
			t.Errorf("Data corrupt: TranferedObjects length mismatch (expected %d, got %d)", len(testData.GetTranferedObjects()), len(unmarshalled.GetTranferedObjects()))
		} else if len(testData.GetTranferedObjects()) > 0 {
			if testData.GetTranferedObjects()[0] != unmarshalled.GetTranferedObjects()[0] {
				t.Errorf("Data corrupt: TranferedObjects[0] mismatch (expected %v, got %v)", testData.GetTranferedObjects()[0], unmarshalled.GetTranferedObjects()[0])
			}
		}

		if len(testData.GetNewValues()) != len(unmarshalled.GetNewValues()) {
			t.Errorf("Data corrupt: ValueMod (new) length mismatch (expected %d, got %d)", len(testData.GetNewValues()), len(unmarshalled.GetNewValues()))
		} else if len(testData.GetNewValues()) > 0 {
			vm1 := testData.GetNewValues()[0]
			vm2 := unmarshalled.GetNewValues()[0]
			if vm1.GetIsDeleting() != vm2.GetIsDeleting() {
				t.Errorf("Data corrupt: ValueMod[0].IsDeleting mismatch (expected %v, got %v)",
					vm1.GetIsDeleting(), vm2.GetIsDeleting())
			}
			if vm1.GetParentObjectId() != vm2.GetParentObjectId() {
				t.Errorf("Data corrupt: ValueMod[0].ParentObjectId mismatch (expected %v, got %v)",
					vm1.GetParentObjectId(), vm2.GetParentObjectId())
			}
			if vm1.GetPersistentMemoryDescriptor() == vm2.GetPersistentMemoryDescriptor() {
				t.Errorf("Data corrupt: ValueMod[0].PersistentMemoryDescriptor mismatch MUST NOT BE SERIALIZED!!! (%v == %v)",
					vm1.GetPersistentMemoryDescriptor(), vm2.GetPersistentMemoryDescriptor())
			}
			if vm1.GetValueId() != vm2.GetValueId() {
				t.Errorf("Data corrupt: ValueMod[0].GetValueId mismatch (expected %v, got %v)",
					vm1.GetValueId(), vm2.GetValueId())
			}
		}

		if len(testData.GetModValues()) != len(unmarshalled.GetModValues()) {
			t.Errorf("Data corrupt: ValueMod (mod) length mismatch (expected %d, got %d)", len(testData.GetModValues()), len(unmarshalled.GetModValues()))
		} else if len(testData.GetModValues()) > 0 {
			vm1 := testData.GetModValues()[0]
			vm2 := unmarshalled.GetModValues()[0]

			if vm1.GetIsDeleting() != vm2.GetIsDeleting() {
				t.Errorf("Data corrupt: ValueMod[0].IsDeleting mismatch (expected %v, got %v)",
					vm1.GetIsDeleting(), vm2.GetIsDeleting())
			}

			if vm1.GetArrayDescriptor() != vm2.GetArrayDescriptor() {
				t.Errorf("Data corrupt: ValueMod[0].GetArrayDescriptor mismatch (expected %v, got %v)",
					vm1.GetArrayDescriptor(), vm2.GetArrayDescriptor())
			}
		}

		if tptr != uptr {
			t.Errorf("Data corrupt: tptr mismatch (expected %v, got %v)", tptr, uptr)
		}
		if tlen != ulen {
			t.Errorf("Data corrupt: tlen mismatch (expected %v, got %v)", tlen, ulen)
		}
		if rtptr != ruptr {
			t.Errorf("Data corrupt: rtptr mismatch (expected %v, got %v)", rtptr, ruptr)
		}
		if rtlen != rulen {
			t.Errorf("Data corrupt: rtlen mismatch (expected %v, got %v)", rtlen, rulen)
		}

		if len(testData.GetRpcs()) != len(unmarshalled.GetRpcs()) {
			t.Errorf("Data corrupt: Rpcs length mismatch (expected %d, got %d)", len(testData.GetRpcs()), len(unmarshalled.GetRpcs()))
		} else if len(testData.GetRpcs()) > 0 {
			rpc1 := testData.GetRpcs()[0]
			rpc2 := unmarshalled.GetRpcs()[0]
			if rpc1.GetMethodId() != rpc2.GetMethodId() {
				t.Errorf("Data corrupt: Rpcs[0].MethodId mismatch (expected %v, got %v)", rpc1.GetMethodId(), rpc2.GetMethodId())
			}
			if rpc1.GetObjectId() != rpc2.GetObjectId() {
				t.Errorf("Data corrupt: Rpcs[0].ObjectId mismatch (expected %v, got %v)", rpc1.GetObjectId(), rpc2.GetObjectId())
			}
			if rpc1.GetSubObjectId() != rpc2.GetSubObjectId() {
				t.Errorf("Data corrupt: Rpcs[0].SubObjectId mismatch (expected %v, got %v)", rpc1.GetSubObjectId(), rpc2.GetSubObjectId())
			}
			if rpc1.GetRpcType() != rpc2.GetRpcType() {
				t.Errorf("Data corrupt: Rpcs[0].RpcType mismatch (expected %v, got %v)", rpc1.GetRpcType(), rpc2.GetRpcType())
			}
			if rpc1.GetTargetClient() != rpc2.GetTargetClient() {
				t.Errorf("Data corrupt: Rpcs[0].TargetClient mismatch (expected %v, got %v)", rpc1.GetTargetClient(), rpc2.GetTargetClient())
			}
		}

		if len(marshalled) > 51 {
			t.Errorf("Too big %v", len(marshalled))
		}

		//log.Printf("Marshalled tick packet len is %v bytes length string presentation {%s}", len(marshalled), string(marshalled))
	}
}

func TestBufferRace(t *testing.T) {
	rpcCall := gameentities.NewRpcCall(11, 22, 1, 2, 3, 4, 1)
	marshalled, merr := marshaller.NewMessagePackMarshaller().Marshal(&rpcCall)

	if merr != nil {
		t.Error(merr)
	}

	var rpcCopy gameentities.RpcCall

	err := marshaller.NewMessagePackMarshaller().Unmarshal(marshalled, &rpcCopy)

	if err != nil {
		t.Error(err)
	}

	for i := range marshalled {
		marshalled[i] = 0
	}

	rptr, rlen := rpcCopy.GetDescriptors()

	if rpcCopy.GetObjectId() != 1 ||
		rpcCopy.GetTargetClient() != 2 ||
		rpcCopy.GetSubObjectId() != 3 ||
		rpcCopy.GetRpcType() != 4 ||
		rpcCopy.GetMethodId() != 1 ||
		rptr != 11 ||
		rlen != 22 {
		t.Error("Data corrupted!")
	}
}

func TestUnreliable(t *testing.T) {
	mrsh := marshaller.NewMessagePackMarshaller()

	for range 1_000_000 {
		tick := datatransferobjects.NewGameUnreliableTickPacket(0, []gameentities.Tranform{gameentities.NewTransform(gameentities.NewGameObject(1, 2, 3, 4, gameentities.NewVector(5, 6, 7), gameentities.NewVector(8, 9, 10)))}, 1)

		buf := &bytes.Buffer{}
		err := mrsh.MarshalNonAlloc(buf, tick)
		marshalled := buf.Bytes()

		if err != nil {
			t.Error(err)
		}

		var unmarshalled datatransferobjects.GameUnreliableTickPacket
		err = mrsh.Unmarshal(marshalled, &unmarshalled)

		if err != nil {
			t.Error(err)
		}

		if unmarshalled.GetTick() != tick.GetTick() ||
			unmarshalled.GetSourceClient() != tick.GetSourceClient() ||
			unmarshalled.GetMovedObjects()[0].GetObjectId() != tick.GetMovedObjects()[0].GetObjectId() ||
			unmarshalled.GetMovedObjects()[0].GetPosition().GetX() != tick.GetMovedObjects()[0].GetPosition().GetX() ||
			unmarshalled.GetMovedObjects()[0].GetPosition().GetY() != tick.GetMovedObjects()[0].GetPosition().GetY() ||
			unmarshalled.GetMovedObjects()[0].GetPosition().GetZ() != tick.GetMovedObjects()[0].GetPosition().GetZ() ||
			unmarshalled.GetMovedObjects()[0].GetRotation().GetX() != tick.GetMovedObjects()[0].GetRotation().GetX() ||
			unmarshalled.GetMovedObjects()[0].GetRotation().GetY() != tick.GetMovedObjects()[0].GetRotation().GetY() ||
			unmarshalled.GetMovedObjects()[0].GetRotation().GetZ() != tick.GetMovedObjects()[0].GetRotation().GetZ() ||
			len(unmarshalled.GetMovedObjects()) != len(tick.GetMovedObjects()) {
			t.Error("Data corrupt")
		}
	}
}

func TestUnrSize(t *testing.T) {
	mrsh := marshaller.NewMessagePackMarshaller()
	transforms := []gameentities.Tranform{}

	for j := range 70 {
		i := uint32(j)
		i16 := uint16(j)
		f32 := float32(j)
		transforms = append(transforms, gameentities.NewTransform(gameentities.NewGameObject(1*i, 2*i, 3*i16, 4*i16, gameentities.NewVector(5*f32, 6*f32, 7*f32), gameentities.NewVector(8*f32, 9*f32, 10*f32))))
	}

	tick := datatransferobjects.NewGameUnreliableTickPacket(0, transforms, 1)
	b, _ := mrsh.Marshal(tick)
	p := make([]byte, len(b)*2)
	w, _ := lz4.CompressBlock(b, p, nil)
	p = p[:w]

	log.Println(len(b), len(p))

	v := gameentities.NewVector(2.1, 2.1, 2.1)
	mv, e := mrsh.Marshal(&v)

	log.Println(len(mv), mv, e)
}

type TestPacket struct {
	ID     uint32
	Value  int64
	Name   string
	Values []int
}

func TestUnmarshalParallel(t *testing.T) {
	m := marshaller.NewMessagePackMarshaller()

	const goroutines = 1024
	const iterations = 5000

	packets := make([][]byte, goroutines)
	expected := make([]TestPacket, goroutines)

	for i := 0; i < goroutines; i++ {
		expected[i] = TestPacket{
			ID:     uint32(i),
			Value:  int64(i * 1000),
			Name:   "goroutine-" + string(rune(i)),
			Values: []int{i, i * 2, i * 3, i * 4},
		}

		data, err := m.Marshal(expected[i])
		if err != nil {
			t.Fatal(err)
		}

		packets[i] = data
	}

	var wg sync.WaitGroup

	wg.Add(goroutines)

	for g := 0; g < goroutines; g++ {
		g := g

		go func() {
			defer wg.Done()

			var dst TestPacket

			for i := 0; i < iterations; i++ {
				if err := m.Unmarshal(packets[g], &dst); err != nil {
					t.Error(err)
				}

				if dst.ID != expected[g].ID ||
					dst.Value != expected[g].Value ||
					dst.Name != expected[g].Name ||
					len(dst.Values) != len(expected[g].Values) {
					t.Errorf("decoded invalid data: %+v expected %+v", dst, expected[g])
				}

				for j := range dst.Values {
					if dst.Values[j] != expected[g].Values[j] {
						t.Error("slice mismatch")
					}
				}
			}
		}()
	}

	wg.Wait()
}

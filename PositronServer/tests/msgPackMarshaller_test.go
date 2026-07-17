package tests

import (
	"bytes"
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	"positron/internal/marshaller"
	"testing"
)

func TestUnmarshalling(t *testing.T) {
	for range 1 {
		obj := gameentities.NewGameObject(1, 2, 3, 4, *gameentities.NewVector(5, 6, 7), *gameentities.NewVector(8, 9, 10))
		val := &gameentities.NetValue{}
		val.MarkAsDeleting()
		val.ModifyPayload([]byte("jopa"))

		rpc := gameentities.NewRpcCall(11, 12, 13, 14, 1, []byte("fff"), false)

		testData := datatransferobjects.NewTickPacket(1, 15, 16, []*gameentities.GameObject{obj}, []uint32{17}, []uint32{18}, []*gameentities.NetValue{val}, []*gameentities.RpcCall{rpc})

		buf := &bytes.Buffer{}
		err := marshaller.NewMessagePackMarshaller().MarshalNonAlloc(buf, testData)
		marshalled := buf.Bytes()

		if err != nil {
			t.Error(err)
		}

		var unmarshalled datatransferobjects.GameTickPacket
		err = marshaller.NewMessagePackMarshaller().Unmarshal(marshalled, &unmarshalled)

		if err != nil {
			t.Error(err)
		}

		if testData.GetTick() != unmarshalled.GetTick() ||
			testData.GetHost() != unmarshalled.GetHost() ||
			testData.GetSourceClient() != unmarshalled.GetSourceClient() ||
			testData.GetNewObjects()[0].GetId() != unmarshalled.GetNewObjects()[0].GetId() ||
			testData.GetNewObjects()[0].GetOwnerId() != unmarshalled.GetNewObjects()[0].GetOwnerId() ||
			testData.GetNewObjects()[0].GetAssetIndex() != unmarshalled.GetNewObjects()[0].GetAssetIndex() ||
			testData.GetNewObjects()[0].GetCreationId() != unmarshalled.GetNewObjects()[0].GetCreationId() ||
			testData.GetNewObjects()[0].GetPosition().GetX() != unmarshalled.GetNewObjects()[0].GetPosition().GetX() ||
			testData.GetNewObjects()[0].GetPosition().GetY() != unmarshalled.GetNewObjects()[0].GetPosition().GetY() ||
			testData.GetNewObjects()[0].GetPosition().GetZ() != unmarshalled.GetNewObjects()[0].GetPosition().GetZ() ||
			testData.GetNewObjects()[0].GetRotation().GetX() != unmarshalled.GetNewObjects()[0].GetRotation().GetX() ||
			testData.GetNewObjects()[0].GetRotation().GetY() != unmarshalled.GetNewObjects()[0].GetRotation().GetY() ||
			testData.GetNewObjects()[0].GetRotation().GetZ() != unmarshalled.GetNewObjects()[0].GetRotation().GetZ() ||
			testData.GetRemovedObjects()[0] != unmarshalled.GetRemovedObjects()[0] ||
			testData.GetTranferedObjects()[0] != unmarshalled.GetTranferedObjects()[0] ||
			testData.GetValueMod()[0].GetIsDeleting() != unmarshalled.GetValueMod()[0].GetIsDeleting() ||
			testData.GetValueMod()[0].GetParentObjectId() != unmarshalled.GetValueMod()[0].GetParentObjectId() ||
			string(testData.GetValueMod()[0].GetPayload()) != string(unmarshalled.GetValueMod()[0].GetPayload()) ||
			testData.GetValueMod()[0].GetValueId() != unmarshalled.GetValueMod()[0].GetValueId() ||
			string(testData.GetRpcs()[0].GetArgs()) != string(unmarshalled.GetRpcs()[0].GetArgs()) ||
			testData.GetRpcs()[0].GetMethodId() != unmarshalled.GetRpcs()[0].GetMethodId() ||
			testData.GetRpcs()[0].GetObjectId() != unmarshalled.GetRpcs()[0].GetObjectId() ||
			testData.GetRpcs()[0].GetSubObjectId() != unmarshalled.GetRpcs()[0].GetSubObjectId() ||
			testData.GetRpcs()[0].GetRpcType() != unmarshalled.GetRpcs()[0].GetRpcType() ||
			testData.GetRpcs()[0].GetTargetClient() != unmarshalled.GetRpcs()[0].GetTargetClient() ||
			len(testData.GetNewObjects()) != len(unmarshalled.GetNewObjects()) ||
			len(testData.GetRemovedObjects()) != len(unmarshalled.GetRemovedObjects()) ||
			len(testData.GetTranferedObjects()) != len(unmarshalled.GetTranferedObjects()) ||
			len(testData.GetRpcs()) != len(unmarshalled.GetRpcs()) ||
			len(testData.GetValueMod()) != len(unmarshalled.GetValueMod()) {
			t.Error("Data corrupt")
		}

		if len(marshalled) > 51 {
			t.Errorf("Too big %v", len(marshalled))
		}

		//log.Printf("Marshalled tick packet len is %v bytes length string presentation {%s}", len(marshalled), string(marshalled))
	}
}

func TestBufferRace(t *testing.T) {
	rpcCall := gameentities.NewRpcCall(1, 2, 3, 4, 1, []byte("A"), false)
	marshalled, _ := marshaller.NewMessagePackMarshaller().Marshal(rpcCall)
	var rpcCopy gameentities.RpcCall

	err := marshaller.NewMessagePackMarshaller().Unmarshal(marshalled, &rpcCopy)

	if err != nil {
		t.Error(err)
	}

	for i := range marshalled {
		marshalled[i] = 0
	}

	if rpcCall.GetObjectId() != 1 ||
		rpcCall.GetTargetClient() != 2 ||
		rpcCall.GetSubObjectId() != 3 ||
		rpcCall.GetRpcType() != 4 ||
		rpcCall.GetMethodId() != 1 ||
		(len(rpcCall.GetArgs()) != 1 || rpcCall.GetArgs()[0] != []byte("A")[0]) {
		t.Error("Data corrupted!")
	}
}

func TestUnreliable(t *testing.T) {
	for range 100_000 {
		tick := datatransferobjects.NewGameUnreliableTickPacket(0, []*gameentities.Tranform{gameentities.NewTransform(gameentities.NewGameObject(1, 2, 3, 4, *gameentities.NewVector(5, 6, 7), *gameentities.NewVector(8, 9, 10)))}, 1)

		buf := &bytes.Buffer{}
		err := marshaller.NewMessagePackMarshaller().MarshalNonAlloc(buf, tick)
		marshalled := buf.Bytes()

		if err != nil {
			t.Error(err)
		}

		var unmarshalled datatransferobjects.GameUnreliableTickPacket
		err = marshaller.NewMessagePackMarshaller().Unmarshal(marshalled, &unmarshalled)

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

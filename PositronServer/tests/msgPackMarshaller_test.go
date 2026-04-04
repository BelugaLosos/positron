package tests

import (
	datatransferobjects "positron/game/dataTransferObjects"
	gameentities "positron/game/gameEntities"
	"positron/internal/marshaller"
	"testing"
)

func TestUnmarshalling(t *testing.T) {
	for range 100_000 {
		obj := gameentities.NewGameObject(1, 2, 3, 4, *gameentities.NewVector(5, 6, 7), *gameentities.NewVector(8, 9, 10))
		val := &gameentities.NetValue{}
		val.MarkAsDeleting()
		val.ModifyPayload([]byte("jopa"))

		rpc := gameentities.NewRpcCall(11, 12, 13, 14, "sraka", []byte("fff"))

		testData := datatransferobjects.NewTickPacket(15, 16, []*gameentities.GameObject{obj}, []uint32{17}, []uint32{18}, []*gameentities.NetValue{val}, []*gameentities.RpcCall{rpc})
		marshalled, err := marshaller.NewMessagePackMarshaller().Marshal(testData)

		if err != nil {
			t.Error(err)
		}

		var unmarshalled datatransferobjects.GameTickPacket
		err = marshaller.NewMessagePackMarshaller().Unmarshal(marshalled, &unmarshalled)

		if err != nil {
			t.Error(err)
		}

		if testData.GetHost() != unmarshalled.GetHost() ||
			testData.GetSourceClient() != unmarshalled.GetSourceClient() ||
			testData.GetNewObjects()[0].GetId() != unmarshalled.GetNewObjects()[0].GetId() ||
			testData.GetNewObjects()[0].GetOwnerId() != unmarshalled.GetNewObjects()[0].GetOwnerId() ||
			testData.GetNewObjects()[0].GetAssetIndex() != unmarshalled.GetNewObjects()[0].GetAssetIndex() ||
			testData.GetNewObjects()[0].GetCreationId() != unmarshalled.GetNewObjects()[0].GetCreationId() ||
			testData.GetNewObjects()[0].GetPosition().Cords[0] != unmarshalled.GetNewObjects()[0].GetPosition().Cords[0] ||
			testData.GetNewObjects()[0].GetPosition().Cords[1] != unmarshalled.GetNewObjects()[0].GetPosition().Cords[1] ||
			testData.GetNewObjects()[0].GetPosition().Cords[2] != unmarshalled.GetNewObjects()[0].GetPosition().Cords[2] ||
			testData.GetNewObjects()[0].GetRotation().Cords[0] != unmarshalled.GetNewObjects()[0].GetRotation().Cords[0] ||
			testData.GetNewObjects()[0].GetRotation().Cords[1] != unmarshalled.GetNewObjects()[0].GetRotation().Cords[1] ||
			testData.GetNewObjects()[0].GetRotation().Cords[2] != unmarshalled.GetNewObjects()[0].GetRotation().Cords[2] ||
			testData.GetRemovedObjects()[0] != unmarshalled.GetRemovedObjects()[0] ||
			testData.GetTranferedObjects()[0] != unmarshalled.GetTranferedObjects()[0] ||
			testData.GetValueMod()[0].GetIsDeleting() != unmarshalled.GetValueMod()[0].GetIsDeleting() ||
			testData.GetValueMod()[0].GetParentObjectId() != unmarshalled.GetValueMod()[0].GetParentObjectId() ||
			string(testData.GetValueMod()[0].GetPayload()) != string(unmarshalled.GetValueMod()[0].GetPayload()) ||
			testData.GetValueMod()[0].GetSubObjectId() != unmarshalled.GetValueMod()[0].GetSubObjectId() ||
			testData.GetValueMod()[0].GetValueId() != unmarshalled.GetValueMod()[0].GetValueId() ||
			string(testData.GetRpcs()[0].GetArgs()) != string(unmarshalled.GetRpcs()[0].GetArgs()) ||
			testData.GetRpcs()[0].GetMethodName() != unmarshalled.GetRpcs()[0].GetMethodName() ||
			testData.GetRpcs()[0].GetObjectId() != unmarshalled.GetRpcs()[0].GetObjectId() ||
			testData.GetRpcs()[0].GetSubObjectId() != unmarshalled.GetRpcs()[0].GetSubObjectId() ||
			testData.GetRpcs()[0].GetTarget() != unmarshalled.GetRpcs()[0].GetTarget() ||
			testData.GetRpcs()[0].GetTargetClient() != unmarshalled.GetRpcs()[0].GetTargetClient() ||
			len(testData.GetNewObjects()) != len(unmarshalled.GetNewObjects()) ||
			len(testData.GetRemovedObjects()) != len(unmarshalled.GetRemovedObjects()) ||
			len(testData.GetTranferedObjects()) != len(unmarshalled.GetTranferedObjects()) ||
			len(testData.GetRpcs()) != len(unmarshalled.GetRpcs()) ||
			len(testData.GetValueMod()) != len(unmarshalled.GetValueMod()) {
			t.Error("Data corrupt")
		}

		if len(marshalled) > 141 {
			t.Errorf("Too big %v", len(marshalled))
		}

		//log.Printf("Marshalled tick packet len is %v bytes length string presentation {%s}", len(marshalled), string(marshalled))
	}
}

func TestUnreliable(t *testing.T) {
	for range 100_000 {
		tick := datatransferobjects.NewGameUnreliableTickPacket([]*gameentities.Tranform{gameentities.NewTransform(gameentities.NewGameObject(1, 2, 3, 4, *gameentities.NewVector(5, 6, 7), *gameentities.NewVector(8, 9, 10)))}, 1)
		marshalled, err := marshaller.NewMessagePackMarshaller().Marshal(tick)

		if err != nil {
			t.Error(err)
		}

		var unmarshalled datatransferobjects.GameUnreliableTickPacket
		err = marshaller.NewMessagePackMarshaller().Unmarshal(marshalled, &unmarshalled)

		if err != nil {
			t.Error(err)
		}

		if unmarshalled.GetSourceClient() != tick.GetSourceClient() ||
			unmarshalled.GetMovedObjects()[0].GetObjectId() != tick.GetMovedObjects()[0].GetObjectId() ||
			unmarshalled.GetMovedObjects()[0].GetPosition().Cords[0] != tick.GetMovedObjects()[0].GetPosition().Cords[0] ||
			unmarshalled.GetMovedObjects()[0].GetPosition().Cords[1] != tick.GetMovedObjects()[0].GetPosition().Cords[1] ||
			unmarshalled.GetMovedObjects()[0].GetPosition().Cords[2] != tick.GetMovedObjects()[0].GetPosition().Cords[2] ||
			unmarshalled.GetMovedObjects()[0].GetRotation().Cords[0] != tick.GetMovedObjects()[0].GetRotation().Cords[0] ||
			unmarshalled.GetMovedObjects()[0].GetRotation().Cords[1] != tick.GetMovedObjects()[0].GetRotation().Cords[1] ||
			unmarshalled.GetMovedObjects()[0].GetRotation().Cords[2] != tick.GetMovedObjects()[0].GetRotation().Cords[2] ||
			len(unmarshalled.GetMovedObjects()) != len(tick.GetMovedObjects()) {
			t.Error("Data corrupt")
		}
	}
}

package tests

import (
	gameentities "positron/game/gameEntities"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestAddValue(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	model.AddOrModify(&gameentities.NetValue{})
	mod := model.GetTempMod()

	if len(mod) != 1 {
		t.Error("No any mod while expected")
	} else if mod[0].GetIsDeleting() == true || mod[0].GetValueId() != 0 || mod[0].GetParentObjectId() != 0 || mod[0].GetSubObjectId() != 0 {
		t.Errorf("Mod content error %v", mod[0])
	}
}

func TestReset(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	model.AddOrModify(&gameentities.NetValue{})
	model.ResetTempBuffers()
	mod := model.GetTempMod()

	if len(mod) != 0 {
		t.Error("Iefficient reset")
	}
}

func TestValueMod(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	model.AddOrModify(&gameentities.NetValue{})
	model.ResetTempBuffers()

	if len(model.GetValues()[0].GetPayload()) != 0 {
		t.Error("Unexpected value payload")
	}

	modVal := &gameentities.NetValue{}
	modVal.ModifyPayload([]byte("hello"))

	model.AddOrModify(modVal)

	mod := model.GetTempMod()

	if len(mod) != 1 {
		t.Error("Mod is not collected")
	} else if string(mod[0].GetPayload()) != "hello" {
		t.Error("Data corruption")
	} else if mod[0].GetIsDeleting() == true {
		t.Error("Unexpected deletion")
	}

	model.ResetTempBuffers()

	if len(model.GetTempMod()) != 0 {
		t.Error("Unefficient reset hit")
	}
}

func TestValueDeletion(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	model.AddOrModify(&gameentities.NetValue{})
	model.ResetTempBuffers()

	model.RemoveAllValuesFromObject(0)

	mod := model.GetTempMod()

	if len(mod) != 1 {
		t.Error("Deletion mod is not collected")
	} else if mod[0].GetIsDeleting() == false {
		t.Error("Delete is not efficient")
	}
}

func TestGetValues(t *testing.T) {
	model := roommodels.NewNetValuesModel()

	val := &gameentities.NetValue{}
	val.ModifyPayload([]byte("hello"))

	model.AddOrModify(val)
	model.ResetTempBuffers()

	if len(model.GetValues()) != 1 || string(val.GetPayload()) != "hello" {
		t.Error("Data not registred or corruted")
	}
}

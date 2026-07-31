package tests

import (
	"encoding/binary"
	gameentities "positron/game/gameEntities"
	roommodels "positron/game/room/roomModels"
	"testing"
)

func TestAddValue(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	arenaImitation := make([]byte, 0, 4)
	arenaImitation = binary.BigEndian.AppendUint32(arenaImitation, 1245)

	model.PutTransientDataIncoming(arenaImitation)
	model.AddOrModify(gameentities.NewNetValueAsTransient(0, 4, 1, 2, false))

	modMeta, modArena := model.GetTempMod()

	if len(modMeta) != 1 {
		t.Error("No any mod while expected")
	} else if modMeta[0].GetIsDeleting() == true || modMeta[0].GetValueId() != 2 || modMeta[0].GetParentObjectId() != 1 {
		t.Errorf("Mod content error %v", modMeta[0])
	} else if len(modArena) != 4 {
		t.Errorf("invalid arena len %v", len(modArena))
	} else if binary.BigEndian.Uint32(modArena) != 1245 {
		t.Errorf("arena data corrupted %v", binary.BigEndian.Uint32(modArena))
	}
}

func TestReset(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	arenaImitation := make([]byte, 0, 4)
	arenaImitation = binary.BigEndian.AppendUint32(arenaImitation, 1245)

	model.PutTransientDataIncoming(arenaImitation)
	model.AddOrModify(gameentities.NewNetValueAsTransient(0, 4, 1, 2, false))

	model.ResetTempBuffers()

	modMeta, modArena := model.GetTempMod()

	if len(modMeta) != 0 {
		t.Error("Iefficient reset")
	} else if len(modArena) != 0 {
		t.Error("Arena not resetted")
	}
}

func TestValueMod(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	arenaImitation := make([]byte, 4)
	model.PutTransientDataIncoming(arenaImitation)
	model.AddOrModify(gameentities.NewNetValueAsTransient(0, 4, 1, 2, false))

	model.ResetTempBuffers()

	binary.BigEndian.PutUint32(arenaImitation, 67)
	model.PutTransientDataIncoming(arenaImitation)
	modVal := gameentities.NewNetValueAsTransient(0, 4, 1, 2, false)

	model.AddOrModify(modVal)

	modMeta, modArena := model.GetTempMod()

	if len(modMeta) != 1 {
		t.Errorf("No any mod while expected %v != 1", len(modMeta))
	} else if modMeta[0].GetIsDeleting() == true || modMeta[0].GetValueId() != 2 || modMeta[0].GetParentObjectId() != 1 {
		t.Errorf("Mod content error %v", modMeta[0])
	} else if len(modArena) != 4 {
		t.Errorf("invalid arena len %v", len(modArena))
	} else if binary.BigEndian.Uint32(modArena) != 67 {
		t.Errorf("arena data corrupted %v", binary.BigEndian.Uint32(modArena))
	}

	model.ResetTempBuffers()

	modMeta, modArena = model.GetTempMod()

	if len(modMeta) != 0 {
		t.Error("Iefficient reset")
	} else if len(modArena) != 0 {
		t.Error("Arena not resetted")
	}
}

func TestValueDeletion(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	arenaImitation := make([]byte, 4)
	model.PutTransientDataIncoming(arenaImitation)

	for i := range uint16(10) {
		model.AddOrModify(gameentities.NewNetValueAsTransient(0, 4, 1, i, false))
	}

	for i := range uint16(10) {
		model.AddOrModify(gameentities.NewNetValueAsTransient(0, 4, 2, i, false))
	}

	model.ResetTempBuffers()

	model.RemoveAllValuesFromObject(1)

	modMeta, modArena := model.GetTempMod()

	if len(modMeta) != 10 {
		t.Errorf("Deletion mod is not collected %v", len(modMeta))
	} else if modMeta[0].GetIsDeleting() == false {
		t.Error("Delete is not efficient")
	} else if len(modArena) != 0 {
		t.Errorf("Unexpected arena %v", modArena)
	}
}

func TestGetValues(t *testing.T) {
	model := roommodels.NewNetValuesModel()
	arenaImitation := make([]byte, 0, 4)
	arenaImitation = binary.BigEndian.AppendUint32(arenaImitation, 1245)

	model.PutTransientDataIncoming(arenaImitation)
	model.AddOrModify(gameentities.NewNetValueAsTransient(0, 4, 1, 2, false))

	model.ResetTempBuffers()

	meta, arena := model.GetValues()
	dptr, dlen := meta[0].GetTransientMemoryDescriptor()

	if len(meta) != 1 || binary.BigEndian.Uint32(arena[dptr:dlen]) != 1245 {
		t.Error("data corruption")
	}
}

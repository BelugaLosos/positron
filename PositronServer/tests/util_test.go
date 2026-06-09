package tests

import (
	gameentities "positron/game/gameEntities"
	"positron/util"
	"testing"
)

func TestDistanceVector(t *testing.T) {
	a := gameentities.NewVector(0, 10, 0)
	b := gameentities.NewVector(0, 0, 0)

	dist := util.PointsDistance(*a, *b)

	if dist != 10 {
		t.Errorf("Distance not valid: %v", dist)
	}

	a = gameentities.NewVector(1, 2, 3)
	b = gameentities.NewVector(1, 2, 2)

	dist = util.PointsDistance(*a, *b)

	if dist != 1 {
		t.Errorf("Distance not valid: %v", dist)
	}
}

func TestVectorEquals(t *testing.T) {
	a := gameentities.NewVector(1, 2, 3)
	b := gameentities.NewVector(6, 5, 4)

	if util.VetorsEquals(*a, *b) == true {
		t.Error("Math is broken")
	}

	a = gameentities.NewVector(1, 2, 3)
	b = gameentities.NewVector(1, 2, 3)

	if util.VetorsEquals(*a, *b) == false {
		t.Error("Math is broken")
	}
}

func TestPacketGluing(t *testing.T) {
	rawData := []byte{0x1, 0x2, 0x3, 0x4, 0x5, 0x67}

	referenceUncompressed := []byte{0xFF, 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x67}
	uncompressed := util.GlueDataToOptions(0xFF, false, uint32(len(rawData)), rawData)

	if !sliceEquals(referenceUncompressed, uncompressed) {
		t.Errorf("Invalid gluing of uncompressed packet %v %v", referenceUncompressed, uncompressed)
	}

	referenceCompressed := []byte{0xFF, 0x1, 0x0, 0x0, 0x0, 0x8, 0x1, 0x2, 0x3, 0x4, 0x5, 0x67}
	compressed := util.GlueDataToOptions(0xFF, true, uint32(8), rawData)

	if !sliceEquals(referenceCompressed, compressed) {
		t.Errorf("Invalid gluing of compressed packet %v %v", referenceCompressed, compressed)
	}
}

func TestDeconstructPacketUncompressed(t *testing.T) {
	rawData := []byte{0x1, 0x2, 0x3, 0x4, 0x5, 0x67}
	eventT, isCompressed, sourceLen, data := util.DeconstructPacket(util.GlueDataToOptions(0xFF, false, uint32(len(rawData)), rawData))

	if eventT != 0xFF || isCompressed == true || sourceLen != uint32(len(rawData)) || !sliceEquals(rawData, data) {
		t.Error("Deconstruction of uncompressed packet failed")
	}
}

func TestDeconstructPacketCompressed(t *testing.T) {
	rawData := []byte{0x1, 0x2, 0x3, 0x4, 0x5, 0x67}
	eventT, isCompressed, sourceLen, data := util.DeconstructPacket(util.GlueDataToOptions(0xFF, true, 25, rawData))

	if eventT != 0xFF || isCompressed == false || sourceLen != 25 || !sliceEquals(rawData, data) {
		t.Errorf("Deconstruction of compressed packet failed %v %v %v %v %v Passes: %v %v %v %v", eventT, isCompressed, sourceLen, rawData, data, eventT != 0xFF, isCompressed == true, sourceLen != 25, !sliceEquals(rawData, data))
	}
}

func sliceEquals(a, b []byte) bool {
	if len(a) != len(b) {
		return false
	}
	for i := range a {
		if a[i] != b[i] {
			return false
		}
	}
	return true
}

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

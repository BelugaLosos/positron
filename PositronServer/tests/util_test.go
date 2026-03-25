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

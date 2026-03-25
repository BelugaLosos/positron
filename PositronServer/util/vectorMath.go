package util

import (
	"math"
	gameentities "positron/game/gameEntities"
)

func PointsDistance(a gameentities.Vector3, b gameentities.Vector3) float32 {
	return float32(math.Sqrt(float64(((a.GetX() - b.GetX()) * (a.GetX() - b.GetX())) + ((a.GetY() - b.GetY()) * (a.GetY() - b.GetY())) + ((a.GetZ() - b.GetZ()) * (a.GetZ() - b.GetZ())))))
}

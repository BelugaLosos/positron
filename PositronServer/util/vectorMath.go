package util

import (
	"math"
	gameentities "positron/game/gameEntities"
)

func PointsDistance(a gameentities.Vector3, b gameentities.Vector3) float32 {
	return float32(math.Sqrt(float64(((a.GetX() - b.GetX()) * (a.GetX() - b.GetX())) + ((a.GetY() - b.GetY()) * (a.GetY() - b.GetY())) + ((a.GetZ() - b.GetZ()) * (a.GetZ() - b.GetZ())))))
}

func VetorsEquals(a gameentities.Vector3, b gameentities.Vector3) bool {
	return a.GetX() == b.GetX() && a.GetY() == b.GetY() && a.GetZ() == b.GetZ()
}

func RotationBetweenEulerAngles(a gameentities.Vector3, b gameentities.Vector3) float32 {
	qx1, qy1, qz1, qw1 := Eul2Quat(a)
	qx2, qy2, qz2, qw2 := Eul2Quat(b)

	dot := (qx1 * qx2) + (qy1 * qy2) + (qz1 * qz2) + (qw1 * qw2)
	mdot := math.Max(-1.0, math.Min(1.0, math.Abs(dot)))

	return float32((2.0 * math.Acos(mdot)) * (180 / math.Pi))
}

func Eul2Quat(eul gameentities.Vector3) (x, y, z, w float64) {
	yaw := float64(eul.GetZ())
	pitch := float64(eul.GetY())
	roll := float64(eul.GetX())

	yaw = yaw * math.Pi / 180
	pitch = pitch * math.Pi / 180
	roll = roll * math.Pi / 180

	cy := math.Cos(yaw * 0.5)
	sy := math.Sin(yaw * 0.5)
	cp := math.Cos(pitch * 0.5)
	sp := math.Sin(pitch * 0.5)
	cr := math.Cos(roll * 0.5)
	sr := math.Sin(roll * 0.5)

	w = (cy*cp*cr + sy*sp*sr)
	x = (cy*cp*sr - sy*sp*cr)
	y = (sy*cp*sr + cy*sp*cr)
	z = (sy*cp*cr - cy*sp*sr)

	return x, y, z, w
}

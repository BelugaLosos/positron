package tests

import (
	"positron/game/room"
	"testing"
	"time"
)

func TestRoomClockTickCalculation(t *testing.T) {
	clock := room.NewRoomClock(30)
	clock.RecordNewStartupTime()

	time.Sleep(time.Second)

	ticks := clock.GetTicksAmountSinceStartup()

	if ticks != 30 {
		t.Error("Inaccurate clock")
	}
}

package room

import (
	"sync"
	"time"
)

type RoomClock struct {
	mutex        *sync.RWMutex
	startupTime  time.Time
	tickDuration time.Duration
}

func NewRoomClock(tickrate uint32) *RoomClock {
	return &RoomClock{
		mutex:        &sync.RWMutex{},
		startupTime:  time.Now(),
		tickDuration: time.Second / time.Duration(tickrate),
	}
}

func (r *RoomClock) RecordNewStartupTime() {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.startupTime = time.Now()
}

func (r *RoomClock) GetTicksAmountSinceStartup() uint32 {
	r.mutex.RLock()
	defer r.mutex.RUnlock()

	return uint32(time.Since(r.startupTime) / r.tickDuration)
}

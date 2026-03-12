package eventtypes

const (
	PING            = 0x0
	PONG            = 0x1
	TICK            = 0x2
	UNRELIABLE_TICK = 0x3
	CREATE_ROOM     = 0x4
	ROOM_CREATED    = 0x5
	GET_ALL_ROOMS   = 0x6
	ROOMS_LIST      = 0x7
)

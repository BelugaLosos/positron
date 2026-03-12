package datatransferobjects

type RoomsListResponse struct {
	_msgpack struct{} `msgpack:",as_array"`

	ListElements []*RoomsListElement
}

type RoomsListElement struct {
	_msgpack struct{} `msgpack:",as_array"`

	Name           string
	Uuid           string
	CurrentPlayers int32
	MaxPlayers     int32
}

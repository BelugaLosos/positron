package datatransferobjects

type RoomsListResponse struct {
	_msgpack struct{} `msgpack:",as_array"`

	ListElements []*RoomsListElement
}

type RoomsListElement struct {
	_msgpack struct{} `msgpack:",as_array"`

	Name           string
	Uuid           string
	CurrentPlayers uint32
	MaxPlayers     uint32
	Scene          uint32
	ExternalData   []byte
}

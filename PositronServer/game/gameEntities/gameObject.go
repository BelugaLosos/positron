package gameentities

type GameObject struct {
	_msgpack struct{} `msgpack:",as_array"`

	AssetIndex uint64
	CreationId uint64
	Id         uint32
	Owner      uint32
	Positron   Vector3
	Rotation   Vector3
}

func NewGameObject(id uint32, ownerPeer uint32, assetIndex uint64, creationId uint64, position Vector3, rotation Vector3) *GameObject {
	return &GameObject{
		AssetIndex: assetIndex,
		CreationId: creationId,
		Id:         id,
		Owner:      ownerPeer,
		Positron:   position,
		Rotation:   rotation,
	}
}

func (o *GameObject) GetCreationId() uint64 {
	return o.CreationId
}

func (o *GameObject) GetId() uint32 {
	return o.Id
}

func (o *GameObject) GetOwnerId() uint32 {
	return o.Owner
}

func (o *GameObject) GetAssetIndex() uint64 {
	return o.AssetIndex
}

func (o *GameObject) GetPosition() Vector3 {
	return o.Positron
}

func (o *GameObject) GetRotation() Vector3 {
	return o.Rotation
}

func (o *GameObject) SetIdAndOnwer(id uint32, owner uint32) {
	o.Id = id
	o.Owner = owner
}

func (o *GameObject) Move(position Vector3, rotation Vector3) {
	o.Rotation = rotation
	o.Positron = position
}

type Vector3 struct {
	_msgpack struct{} `msgpack:",as_array"`

	Cords []float32
}

func NewVector(x float32, y float32, z float32) *Vector3 {
	return &Vector3{
		Cords: []float32{x, y, z},
	}
}

func (v *Vector3) GetX() float32 {
	return v.Cords[0]
}

func (v *Vector3) GetY() float32 {
	return v.Cords[1]
}

func (v *Vector3) GetZ() float32 {
	return v.Cords[2]
}

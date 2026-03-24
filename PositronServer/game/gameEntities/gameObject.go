package gameentities

type GameObject struct {
	_msgpack struct{} `msgpack:",as_array"`

	assetIndex uint64
	creationId uint64
	id         uint32
	owner      uint32
	positron   Vector3
	rotation   Vector3
}

func NewGameObject(id uint32, ownerPeer uint32, assetIndex uint64, creationId uint64, position Vector3, rotation Vector3) *GameObject {
	return &GameObject{
		assetIndex: assetIndex,
		creationId: creationId,
		id:         id,
		owner:      ownerPeer,
		positron:   position,
		rotation:   rotation,
	}
}

func (o *GameObject) GetCreationId() uint64 {
	return o.creationId
}

func (o *GameObject) GetId() uint32 {
	return o.id
}

func (o *GameObject) GetOwnerId() uint32 {
	return o.owner
}

func (o *GameObject) GetAssetIndex() uint64 {
	return o.assetIndex
}

func (o *GameObject) GetPosition() Vector3 {
	return o.positron
}

func (o *GameObject) GetRotation() Vector3 {
	return o.rotation
}

func (o *GameObject) SetIdAndOnwer(id uint32, owner uint32) {
	o.id = id
	o.owner = owner
}

func (o *GameObject) Move(position Vector3, rotation Vector3) {
	o.rotation = rotation
	o.positron = position
}

type Vector3 struct {
	cords []float32
}

func NewVector(x float32, y float32, z float32) *Vector3 {
	return &Vector3{
		cords: []float32{x, y, z},
	}
}

func (v *Vector3) GetX() float32 {
	return v.cords[0]
}

func (v *Vector3) GetY() float32 {
	return v.cords[1]
}

func (v *Vector3) GetZ() float32 {
	return v.cords[2]
}

package gameentities

type Tranform struct {
	_msgpack struct{} `msgpack:",as_array"`

	ObjectId uint32
	Position Vector3
	Rotation Vector3
}

func NewTransform(gameObject *GameObject) *Tranform {
	return &Tranform{
		ObjectId: gameObject.Id,
		Position: gameObject.Positron,
		Rotation: gameObject.Rotation,
	}
}

func (t *Tranform) GetObjectId() uint32 {
	return t.ObjectId
}

func (t *Tranform) GetPosition() Vector3 {
	return t.Position
}

func (t *Tranform) GetRotation() Vector3 {
	return t.Rotation
}

func (t *Tranform) Move(position Vector3, rotation Vector3) {
	t.Position = position
	t.Rotation = rotation
}

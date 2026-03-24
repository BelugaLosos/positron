package gameentities

type Tranform struct {
	_msgpack struct{} `msgpack:",as_array"`

	objectId uint32
	position Vector3
	rotation Vector3
}

func NewTransform(gameObject *GameObject) *Tranform {
	return &Tranform{
		objectId: gameObject.id,
		position: gameObject.positron,
		rotation: gameObject.rotation,
	}
}

func (t *Tranform) GetObjectId() uint32 {
	return t.objectId
}

func (t *Tranform) GetPosition() Vector3 {
	return t.position
}

func (t *Tranform) GetRotation() Vector3 {
	return t.rotation
}

func (t *Tranform) Move(position Vector3, rotation Vector3) {
	t.position = position
	t.rotation = rotation
}

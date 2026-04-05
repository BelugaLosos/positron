package gameentities

import "github.com/vmihailenco/msgpack/v5"

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

func (t *Tranform) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.EncodeArrayLen(3)
	err := enc.EncodeUint(uint64(t.ObjectId))

	if err != nil {
		return err
	}

	err = enc.Encode(&t.Position)

	if err != nil {
		return err
	}

	err = enc.Encode(&t.Rotation)

	return err
}

func (t *Tranform) DecodeMsgpack(dec *msgpack.Decoder) error {
	dec.DecodeArrayLen()
	objectId, err := dec.DecodeUint()

	if err != nil {
		return err
	}

	var position Vector3
	err = dec.Decode(&position)

	if err != nil {
		return err
	}

	var rotation Vector3
	err = dec.Decode(&rotation)

	t.ObjectId = uint32(objectId)
	t.Position = position
	t.Rotation = rotation

	return err
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

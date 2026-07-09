package gameentities

import (
	"errors"

	"github.com/vmihailenco/msgpack/v5"
)

type Tranform struct {
	position Vector3
	rotation Vector3
	objectId uint32

	staticScoreCounter int // THIS FIELD MUST BE INGNORED IN SERIALIZATION/DESERIALIZATION OF DATA
}

func NewTransform(gameObject *GameObject) *Tranform {
	return &Tranform{
		objectId: gameObject.GetId(),
		position: gameObject.GetPosition(),
		rotation: gameObject.GetRotation(),
	}
}

func (t *Tranform) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(3)
	err := enc.EncodeUint(uint64(t.objectId))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.Encode(&t.position)

	if err != nil {
		return err
	}

	err = enc.Encode(&t.rotation)

	return err
}

func (t *Tranform) DecodeMsgpack(dec *msgpack.Decoder) error {
	arrLen, arrErr := dec.DecodeArrayLen()
	objectId, err := dec.DecodeUint32()

	if arrErr != nil {
		return arrErr
	}

	if arrLen != 3 {
		return errors.New("Transform arr invalid!")
	}

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

	t.objectId = objectId
	t.position = position
	t.rotation = rotation

	return err
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

func (t *Tranform) Move(pos, rot Vector3) {
	t.position = pos
	t.rotation = rot
}

func (t *Tranform) GetStaticScore() int {
	return t.staticScoreCounter
}

func (t *Tranform) EvaluateStaticScore() {
	t.staticScoreCounter++
}

func (t *Tranform) ResetStaticScore() {
	t.staticScoreCounter = 0
}

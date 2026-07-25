package gameentities

import (
	"errors"
	"fmt"

	"github.com/vmihailenco/msgpack/v5"
)

type GameObject struct {
	positron   Vector3
	rotation   Vector3
	id         uint32
	owner      uint32
	assetIndex uint16
	creationId uint16
}

func NewGameObject(id uint32, ownerPeer uint32, assetIndex uint16, creationId uint16, position Vector3, rotation Vector3) GameObject {
	return GameObject{
		id:         id,
		owner:      ownerPeer,
		assetIndex: assetIndex,
		creationId: creationId,
		positron:   position,
		rotation:   rotation,
	}
}

func (g *GameObject) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(6)
	err := enc.EncodeUint(uint64(g.assetIndex))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.creationId))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.id))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.owner))

	if err != nil {
		return err
	}

	err = g.positron.EncodeMsgpack(enc)

	if err != nil {
		return err
	}

	err = g.rotation.EncodeMsgpack(enc)

	return err
}

func (g *GameObject) DecodeMsgpack(dec *msgpack.Decoder) error {
	arrlen, arrErr := dec.DecodeArrayLen()
	assetIndex, err := dec.DecodeUint16()

	if arrErr != nil {
		return arrErr
	}

	if arrlen != 6 {
		return errors.New("Arrat len of game object is incorrect!")
	}

	if err != nil {
		return err
	}

	CreationId, err := dec.DecodeUint16()

	if err != nil {
		return err
	}

	Id, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	Owner, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	err = g.positron.DecodeMsgpack(dec)

	if err != nil {
		return err
	}

	err = g.rotation.DecodeMsgpack(dec)

	if err != nil {
		return err
	}

	g.assetIndex = assetIndex
	g.creationId = CreationId
	g.id = Id
	g.owner = Owner

	return nil
}

func (o *GameObject) GetCreationId() uint16 {
	return o.creationId
}

func (o *GameObject) GetId() uint32 {
	return o.id
}

func (o *GameObject) GetOwnerId() uint32 {
	return o.owner
}

func (o *GameObject) GetAssetIndex() uint16 {
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
	x float32
	y float32
	z float32
}

func NewVector(x float32, y float32, z float32) Vector3 {
	return Vector3{
		x: x,
		y: y,
		z: z,
	}
}

func (v *Vector3) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)

	if err := enc.EncodeArrayLen(3); err != nil {
		return err
	}

	err := enc.EncodeFloat32(v.x)

	if err != nil {
		return err
	}

	err = enc.EncodeFloat32(v.y)

	if err != nil {
		return err
	}

	err = enc.EncodeFloat32(v.z)

	return err
}

func (v *Vector3) DecodeMsgpack(dec *msgpack.Decoder) error {
	plen, derr := dec.DecodeArrayLen()

	if derr != nil {
		return derr
	}

	if plen != 3 {
		return errors.New("vector3 must contain 3 floats")
	}

	x, errX := dec.DecodeFloat32()
	y, errY := dec.DecodeFloat32()
	z, errZ := dec.DecodeFloat32()

	v.x = x
	v.y = y
	v.z = z

	if errX != nil || errY != nil || errZ != nil {
		return fmt.Errorf("XE: %v, YE: %v, ZE: %v", errX, errY, errZ)
	}

	return nil
}

func (v Vector3) GetX() float32 {
	return v.x
}

func (v Vector3) GetY() float32 {
	return v.y
}

func (v Vector3) GetZ() float32 {
	return v.z
}

package gameentities

import (
	"fmt"

	"github.com/vmihailenco/msgpack/v5"
)

type GameObject struct {
	_msgpack struct{} `msgpack:",as_array"`

	Id         uint32
	Owner      uint32
	AssetIndex uint16
	CreationId uint16
	Positron   Vector3
	Rotation   Vector3
}

func NewGameObject(id uint32, ownerPeer uint32, assetIndex uint16, creationId uint16, position Vector3, rotation Vector3) *GameObject {
	return &GameObject{
		Id:         id,
		Owner:      ownerPeer,
		AssetIndex: assetIndex,
		CreationId: creationId,
		Positron:   position,
		Rotation:   rotation,
	}
}

func (g *GameObject) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.EncodeArrayLen(6)
	err := enc.EncodeUint(uint64(g.AssetIndex))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.CreationId))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.Id))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.Owner))

	if err != nil {
		return err
	}

	err = enc.Encode(&g.Positron)

	if err != nil {
		return err
	}

	err = enc.Encode(&g.Rotation)

	return err
}

func (g *GameObject) DecodeMsgpack(dec *msgpack.Decoder) error {
	dec.DecodeArrayLen()
	assetIndex, err := dec.DecodeUint()

	if err != nil {
		return err
	}

	CreationId, err := dec.DecodeUint()

	if err != nil {
		return err
	}

	Id, err := dec.DecodeUint()

	if err != nil {
		return err
	}

	Owner, err := dec.DecodeUint()

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

	if err != nil {
		return err
	}

	g.AssetIndex = uint16(assetIndex)
	g.CreationId = uint16(CreationId)
	g.Id = uint32(Id)
	g.Owner = uint32(Owner)
	g.Positron = position
	g.Rotation = rotation

	return nil
}

func (o *GameObject) GetCreationId() uint16 {
	return o.CreationId
}

func (o *GameObject) GetId() uint32 {
	return o.Id
}

func (o *GameObject) GetOwnerId() uint32 {
	return o.Owner
}

func (o *GameObject) GetAssetIndex() uint16 {
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

	X float32
	Y float32
	Z float32
}

func NewVector(x float32, y float32, z float32) *Vector3 {
	return &Vector3{
		X: x,
		Y: y,
		Z: z,
	}
}

func (v *Vector3) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.EncodeArrayLen(3)
	err := enc.EncodeFloat32(v.X)

	if err != nil {
		return err
	}

	err = enc.EncodeFloat32(v.Y)

	if err != nil {
		return err
	}

	err = enc.EncodeFloat32(v.Z)

	return err
}

func (v *Vector3) DecodeMsgpack(dec *msgpack.Decoder) error {
	dec.DecodeArrayLen()
	x, errX := dec.DecodeFloat32()
	y, errY := dec.DecodeFloat32()
	z, errZ := dec.DecodeFloat32()

	v.X = x
	v.Y = y
	v.Z = z

	if errX != nil || errY != nil || errZ != nil {
		return fmt.Errorf("XE: %v, YE: %v, ZE: %v", errX, errY, errZ)
	}

	return nil
}

func (v *Vector3) GetX() float32 {
	return v.X
}

func (v *Vector3) GetY() float32 {
	return v.Y
}

func (v *Vector3) GetZ() float32 {
	return v.Z
}

package datatransferobjects

import (
	"errors"
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameTickPacket struct {
	newObjects        []gameentities.GameObject
	newValues         []gameentities.NetValue
	modValues         []gameentities.PersistentNetValue
	rpc               []gameentities.RpcCall
	removedObjects    []uint32
	transferedObjects []uint32
	tick              uint32
	host              uint32
	client            uint32
}

func NewTickPacket(tick uint32, host uint32, sourceClient uint32, newObjects []gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, newValues []gameentities.NetValue, modValues []gameentities.PersistentNetValue, rpc []gameentities.RpcCall) *GameTickPacket {
	return &GameTickPacket{
		tick:              tick,
		host:              host,
		client:            sourceClient,
		newObjects:        newObjects,
		removedObjects:    removedObjects,
		transferedObjects: transferedObjects,
		newValues:         newValues,
		modValues:         modValues,
		rpc:               rpc,
	}
}

func (g *GameTickPacket) ReassignTickPacketData(tick uint32, host uint32, sourceClient uint32, newObjects []gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, newValues []gameentities.NetValue, modValues []gameentities.PersistentNetValue, rpc []gameentities.RpcCall) {
	g.tick = tick
	g.host = host
	g.client = sourceClient
	g.newObjects = newObjects
	g.removedObjects = removedObjects
	g.transferedObjects = transferedObjects
	g.newValues = newValues
	g.modValues = modValues
	g.rpc = rpc
}

func (g *GameTickPacket) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(9)
	err := enc.EncodeUint(uint64(g.tick))
	err = enc.EncodeUint(uint64(g.host))
	err = enc.EncodeUint(uint64(g.client))
	err = enc.EncodeArrayLen(len(g.newObjects))

	if arrErr != nil {
		return arrErr
	}

	for i := range g.newObjects {
		err := g.newObjects[i].EncodeMsgpack(enc)

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.removedObjects))

	for i := range g.removedObjects {
		enc.EncodeUint(uint64(g.removedObjects[i]))
	}

	err = enc.EncodeArrayLen(len(g.transferedObjects))

	for i := range g.transferedObjects {
		err := enc.EncodeUint(uint64(g.transferedObjects[i]))

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.newValues))

	for i := range g.newValues {
		err := g.newValues[i].EncodeMsgpack(enc)

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.modValues))

	for i := range g.modValues {
		err := g.modValues[i].EncodeMsgpack(enc)

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.rpc))

	for i := range g.rpc {
		err := g.rpc[i].EncodeMsgpack(enc)

		if err != nil {
			return err
		}
	}

	if err != nil {
		return err
	}

	return nil
}

func (i *GameTickPacket) DecodeMsgpack(dec *msgpack.Decoder) error {
	arrLen, arrErr := dec.DecodeArrayLen()
	tick, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	host, err := dec.DecodeUint32()

	if arrErr != nil {
		return arrErr
	}

	if arrLen != 9 {
		return errors.New("Tick packet arr invalid!")
	}

	if err != nil {
		return err
	}

	i.tick = tick
	i.host = host

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	client, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	i.client = client

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	newObjectsLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.newObjects = i.newObjects[:cap(i.newObjects)]

	for index := range newObjectsLen {
		if index >= cap(i.newObjects) || index >= len(i.newObjects) {
			var obj gameentities.GameObject
			err = obj.DecodeMsgpack(dec)

			if err != nil {
				return err
			}

			i.newObjects = append(i.newObjects, obj)
		} else {
			err = i.newObjects[index].DecodeMsgpack(dec)

			if err != nil {
				return err
			}
		}
	}

	i.newObjects = i.newObjects[:newObjectsLen]

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	removedObjectsLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.removedObjects = i.removedObjects[:0]

	for range removedObjectsLen {
		removeId, err := dec.DecodeUint32()

		if err != nil {
			return err
		}

		i.removedObjects = append(i.removedObjects, removeId)
	}

	i.removedObjects = i.removedObjects[:removedObjectsLen]

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	transferedObjsLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.transferedObjects = i.transferedObjects[:0]

	for range transferedObjsLen {
		transferedId, err := dec.DecodeUint32()

		if err != nil {
			return err
		}

		i.transferedObjects = append(i.transferedObjects, transferedId)
	}

	i.transferedObjects = i.transferedObjects[:transferedObjsLen]

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	valuesAddLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.newValues = i.newValues[:cap(i.newValues)]

	for index := range valuesAddLen {
		if index >= cap(i.newValues) || index >= len(i.newValues) {
			var value gameentities.NetValue
			err = value.DecodeMsgpack(dec)

			if err != nil {
				return err
			}

			i.newValues = append(i.newValues, value)
		} else {
			err = i.newValues[index].DecodeMsgpack(dec)

			if err != nil {
				return err
			}
		}
	}

	i.newValues = i.newValues[:valuesAddLen]

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	valueModLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.modValues = i.modValues[:cap(i.modValues)]

	for index := range valueModLen {
		if index >= cap(i.modValues) || index >= len(i.modValues) {
			var value gameentities.PersistentNetValue
			err = value.DecodeMsgpack(dec)

			if err != nil {
				return err
			}

			i.modValues = append(i.modValues, value)
		} else {
			err = i.modValues[index].DecodeMsgpack(dec)

			if err != nil {
				return err
			}
		}
	}

	i.modValues = i.modValues[:valueModLen]

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	rpcBufferLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.rpc = i.rpc[:cap(i.rpc)]

	for index := range rpcBufferLen {
		if index >= cap(i.rpc) || index >= len(i.rpc) {
			var rpc gameentities.RpcCall
			err = rpc.DecodeMsgpack(dec)

			if err != nil {
				return err
			}

			i.rpc = append(i.rpc, rpc)
		} else {
			err = i.rpc[index].DecodeMsgpack(dec)

			if err != nil {
				return err
			}
		}
	}

	i.rpc = i.rpc[:rpcBufferLen]

	return nil
}

func (g *GameTickPacket) GetTick() uint32 {
	return g.tick
}

func (g *GameTickPacket) GetHost() uint32 {
	return g.host
}

func (g *GameTickPacket) GetSourceClient() uint32 {
	return g.client
}

func (g *GameTickPacket) GetNewObjects() []gameentities.GameObject {
	return g.newObjects
}

func (g *GameTickPacket) GetRemovedObjects() []uint32 {
	return g.removedObjects
}

func (g *GameTickPacket) GetTranferedObjects() []uint32 {
	return g.transferedObjects
}

func (g *GameTickPacket) GetNewValues() []gameentities.NetValue {
	return g.newValues
}

func (g *GameTickPacket) GetModValues() []gameentities.PersistentNetValue {
	return g.modValues
}

func (g *GameTickPacket) GetRpcs() []gameentities.RpcCall {
	return g.rpc
}

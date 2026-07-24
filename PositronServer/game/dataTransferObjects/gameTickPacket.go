package datatransferobjects

import (
	"errors"
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameTickPacket struct {
	newObjects        []gameentities.GameObject
	valueMod          []gameentities.NetValue
	rpc               []gameentities.RpcCall
	removedObjects    []uint32
	transferedObjects []uint32
	tick              uint32
	host              uint32
	client            uint32
}

func NewTickPacket(tick uint32, host uint32, sourceClient uint32, newObjects []gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, valueMod []gameentities.NetValue, rpc []gameentities.RpcCall) *GameTickPacket {
	return &GameTickPacket{
		tick:              tick,
		host:              host,
		client:            sourceClient,
		newObjects:        newObjects,
		removedObjects:    removedObjects,
		transferedObjects: transferedObjects,
		valueMod:          valueMod,
		rpc:               rpc,
	}
}

func (g *GameTickPacket) ReassignTickPacketData(tick uint32, host uint32, sourceClient uint32, newObjects []gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, valueMod []gameentities.NetValue, rpc []gameentities.RpcCall) {
	g.tick = tick
	g.host = host
	g.client = sourceClient
	g.newObjects = newObjects
	g.removedObjects = removedObjects
	g.transferedObjects = transferedObjects
	g.valueMod = valueMod
	g.rpc = rpc
}

func (g *GameTickPacket) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(8)
	err := enc.EncodeUint(uint64(g.tick))
	err = enc.EncodeUint(uint64(g.host))
	err = enc.EncodeUint(uint64(g.client))
	err = enc.EncodeArrayLen(len(g.newObjects))

	if arrErr != nil {
		return arrErr
	}

	for i := range g.newObjects {
		err := enc.Encode(&g.newObjects[i])

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

	err = enc.EncodeArrayLen(len(g.valueMod))

	for i := range g.valueMod {
		err := enc.Encode(&g.valueMod[i])

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.rpc))

	for i := range g.rpc {
		err := enc.Encode(&g.rpc[i])

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

	if arrLen != 8 {
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
			err = dec.Decode(&obj)

			if err != nil {
				return err
			}

			i.newObjects = append(i.newObjects, obj)
		} else {
			err = dec.Decode(&i.newObjects[index])

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

	valueModLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.valueMod = i.valueMod[:cap(i.valueMod)]

	for index := range valueModLen {
		if index >= cap(i.valueMod) || index >= len(i.valueMod) {
			var value gameentities.NetValue
			err = dec.Decode(&value)

			if err != nil {
				return err
			}

			i.valueMod = append(i.valueMod, value)
		} else {
			err = dec.Decode(&i.valueMod[index])

			if err != nil {
				return err
			}
		}
	}

	i.valueMod = i.valueMod[:valueModLen]

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	rpcBufferLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	i.rpc = i.rpc[:cap(i.rpc)]

	for index := range rpcBufferLen {
		if index >= cap(i.rpc) || index >= len(i.rpc) {
			var rpc gameentities.RpcCall
			err = dec.Decode(&rpc)

			if err != nil {
				return err
			}

			i.rpc = append(i.rpc, rpc)
		} else {
			err = dec.Decode(&i.rpc[index])

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

func (g *GameTickPacket) GetValueMod() []gameentities.NetValue {
	return g.valueMod
}

func (g *GameTickPacket) GetRpcs() []gameentities.RpcCall {
	return g.rpc
}

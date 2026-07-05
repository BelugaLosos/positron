package datatransferobjects

import (
	"errors"
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameTickPacket struct {
	Tick               uint32
	Host               uint32
	Client             uint32
	NewObjects         []*gameentities.GameObject
	RemovedObjects     []uint32
	TransferedObjects  []uint32
	ValueMod           []*gameentities.NetValue
	Rpc                []*gameentities.RpcCall
	RequestedOwnership []uint32
}

func NewTickPacket(tick uint32, host uint32, sourceClient uint32, newObjects []*gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, valueMod []*gameentities.NetValue, rpc []*gameentities.RpcCall, requestedOwnership []uint32) *GameTickPacket {
	return &GameTickPacket{
		Tick:               tick,
		Host:               host,
		Client:             sourceClient,
		NewObjects:         newObjects,
		RemovedObjects:     removedObjects,
		TransferedObjects:  transferedObjects,
		ValueMod:           valueMod,
		Rpc:                rpc,
		RequestedOwnership: requestedOwnership,
	}
}

func (g *GameTickPacket) ReassignTickPacketData(tick uint32, host uint32, sourceClient uint32, newObjects []*gameentities.GameObject, removedObjects []uint32, transferedObjects []uint32, valueMod []*gameentities.NetValue, rpc []*gameentities.RpcCall, requestedOwnership []uint32) {
	g.Tick = tick
	g.Host = host
	g.Client = sourceClient
	g.NewObjects = newObjects
	g.RemovedObjects = removedObjects
	g.TransferedObjects = transferedObjects
	g.ValueMod = valueMod
	g.Rpc = rpc
	g.RequestedOwnership = requestedOwnership
}

func (g *GameTickPacket) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(9)
	err := enc.EncodeUint(uint64(g.Tick))
	err = enc.EncodeUint(uint64(g.Host))
	err = enc.EncodeUint(uint64(g.Client))
	err = enc.EncodeArrayLen(len(g.NewObjects))

	if arrErr != nil {
		return arrErr
	}

	for i := range g.NewObjects {
		err := enc.Encode(g.NewObjects[i])

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.RemovedObjects))

	for i := range g.RemovedObjects {
		enc.EncodeUint(uint64(g.RemovedObjects[i]))
	}

	err = enc.EncodeArrayLen(len(g.TransferedObjects))

	for i := range g.TransferedObjects {
		err := enc.EncodeUint(uint64(g.TransferedObjects[i]))

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.ValueMod))

	for i := range g.ValueMod {
		err := enc.Encode(g.ValueMod[i])

		if err != nil {
			return err
		}
	}

	err = enc.EncodeArrayLen(len(g.Rpc))

	for i := range g.Rpc {
		err := enc.Encode(g.Rpc[i])

		if err != nil {
			return err
		}
	}

	if err != nil {
		return err
	}

	err = enc.EncodeArrayLen(len(g.RequestedOwnership))

	for i := range g.RequestedOwnership {
		err := enc.EncodeUint(uint64(g.RequestedOwnership[i]))

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

	i.Tick = tick
	i.Host = host

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	client, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	i.Client = client

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	newObjectsLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	objsArray := make([]*gameentities.GameObject, newObjectsLen)

	for i := range newObjectsLen {
		var obj gameentities.GameObject
		err = dec.Decode(&obj)

		if err != nil {
			return err
		}

		objsArray[i] = &obj
	}

	i.NewObjects = objsArray

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	removedObjectsLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	removedObjsArray := make([]uint32, removedObjectsLen)

	for i := range removedObjsArray {
		removeId, err := dec.DecodeUint32()

		if err != nil {
			return err
		}

		removedObjsArray[i] = removeId
	}

	i.RemovedObjects = removedObjsArray

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	transferedObjsLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	transferedObjects := make([]uint32, transferedObjsLen)

	for i := range transferedObjsLen {
		transferedId, err := dec.DecodeUint32()

		if err != nil {
			return err
		}

		transferedObjects[i] = transferedId
	}

	i.TransferedObjects = transferedObjects

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	valueModLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	valueMod := make([]*gameentities.NetValue, valueModLen)

	for i := range valueModLen {
		var value gameentities.NetValue
		err = dec.Decode(&value)

		if err != nil {
			return err
		}

		valueMod[i] = &value
	}

	i.ValueMod = valueMod

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	rpcBufferLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	rpcBuffer := make([]*gameentities.RpcCall, rpcBufferLen)

	for i := range rpcBuffer {
		var rpc gameentities.RpcCall
		err = dec.Decode(&rpc)

		if err != nil {
			return err
		}

		rpcBuffer[i] = &rpc
	}

	i.Rpc = rpcBuffer

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	requestedOwnershipLen, err := dec.DecodeArrayLen()

	if err != nil {
		return err
	}

	requestedOwnershipBuffer := make([]uint32, requestedOwnershipLen)

	for i := range requestedOwnershipBuffer {
		oid, err := dec.DecodeUint32()

		if err != nil {
			return err
		}

		requestedOwnershipBuffer[i] = oid
	}

	i.RequestedOwnership = requestedOwnershipBuffer

	return nil
}

func (g *GameTickPacket) GetTick() uint32 {
	return g.Tick
}

func (g *GameTickPacket) GetHost() uint32 {
	return g.Host
}

func (g *GameTickPacket) GetSourceClient() uint32 {
	return g.Client
}

func (g *GameTickPacket) GetNewObjects() []*gameentities.GameObject {
	return g.NewObjects
}

func (g *GameTickPacket) GetRemovedObjects() []uint32 {
	return g.RemovedObjects
}

func (g *GameTickPacket) GetTranferedObjects() []uint32 {
	return g.TransferedObjects
}

func (g *GameTickPacket) GetValueMod() []*gameentities.NetValue {
	return g.ValueMod
}

func (g *GameTickPacket) GetRpcs() []*gameentities.RpcCall {
	return g.Rpc
}

func (g *GameTickPacket) GetRequestedOwnershipBuffer() []uint32 {
	return g.RequestedOwnership
}

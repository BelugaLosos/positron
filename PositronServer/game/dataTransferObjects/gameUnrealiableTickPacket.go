package datatransferobjects

import (
	"errors"
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameUnreliableTickPacket struct {
	Tick         uint32
	SourceClient uint32
	MovedObjects []*gameentities.Tranform
}

func NewGameUnreliableTickPacket(tick uint32, movedObjects []*gameentities.Tranform, sourceClient uint32) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		Tick:         tick,
		SourceClient: sourceClient,
		MovedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) ReassignUnreliableTickPacket(tick uint32, movedObjects []*gameentities.Tranform, sourceClient uint32) {
	g.Tick = tick
	g.SourceClient = sourceClient
	g.MovedObjects = movedObjects
}

func (g *GameUnreliableTickPacket) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(3)
	err := enc.EncodeUint(uint64(g.Tick))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.SourceClient))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeArrayLen(len(g.MovedObjects))

	for i := range g.MovedObjects {
		err := enc.Encode(g.MovedObjects[i])

		if err != nil {
			return err
		}
	}

	return err
}

func (g *GameUnreliableTickPacket) DecodeMsgpack(dec *msgpack.Decoder) error {
	arrLen, arrErr := dec.DecodeArrayLen()
	tick, err := dec.DecodeUint32()

	if err != nil {
		return err
	}

	sourceId, err := dec.DecodeUint32()

	if arrErr != nil {
		return arrErr
	}

	if arrLen != 3 {
		return errors.New("UnreliableTick arr invalid!")
	}

	if err != nil {
		return err
	}

	movedLen, err := dec.DecodeArrayLen()
	moved := make([]*gameentities.Tranform, movedLen)

	for i := range movedLen {
		var obj gameentities.Tranform
		err := dec.Decode(&obj)

		if err != nil {
			return err
		}

		moved[i] = &obj
	}

	g.Tick = tick
	g.SourceClient = sourceId
	g.MovedObjects = moved

	return err
}

func (g *GameUnreliableTickPacket) GetTick() uint32 {
	return g.Tick
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []*gameentities.Tranform {
	return g.MovedObjects
}

func (g *GameUnreliableTickPacket) GetSourceClient() uint32 {
	return g.SourceClient
}

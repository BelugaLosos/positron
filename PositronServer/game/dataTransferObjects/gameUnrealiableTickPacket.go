package datatransferobjects

import (
	"errors"
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameUnreliableTickPacket struct {
	movedObjects []*gameentities.Tranform
	tick         uint32
	sourceClient uint32
}

func NewGameUnreliableTickPacket(tick uint32, movedObjects []*gameentities.Tranform, sourceClient uint32) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		tick:         tick,
		sourceClient: sourceClient,
		movedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) ReassignUnreliableTickPacket(tick uint32, movedObjects []*gameentities.Tranform, sourceClient uint32) {
	g.tick = tick
	g.sourceClient = sourceClient
	g.movedObjects = movedObjects
}

func (g *GameUnreliableTickPacket) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)
	arrErr := enc.EncodeArrayLen(3)
	err := enc.EncodeUint(uint64(g.tick))

	if err != nil {
		return err
	}

	err = enc.EncodeUint(uint64(g.sourceClient))

	if arrErr != nil {
		return arrErr
	}

	if err != nil {
		return err
	}

	err = enc.EncodeArrayLen(len(g.movedObjects))

	for i := range g.movedObjects {
		err := enc.Encode(g.movedObjects[i])

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

	g.tick = tick
	g.sourceClient = sourceId
	g.movedObjects = moved

	return err
}

func (g *GameUnreliableTickPacket) GetTick() uint32 {
	return g.tick
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []*gameentities.Tranform {
	return g.movedObjects
}

func (g *GameUnreliableTickPacket) GetSourceClient() uint32 {
	return g.sourceClient
}

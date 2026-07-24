package datatransferobjects

import (
	"errors"
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameUnreliableTickPacket struct {
	movedObjects []gameentities.Tranform
	tick         uint32
	sourceClient uint32
}

func NewGameUnreliableTickPacket(tick uint32, movedObjects []gameentities.Tranform, sourceClient uint32) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		tick:         tick,
		sourceClient: sourceClient,
		movedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) ReassignUnreliableTickPacket(tick uint32, movedObjects []gameentities.Tranform, sourceClient uint32) {
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
		err := enc.Encode(&g.movedObjects[i])

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

	g.movedObjects = g.movedObjects[:cap(g.movedObjects)]

	for i := range movedLen {
		if i >= cap(g.movedObjects) || i >= len(g.movedObjects) {
			var obj gameentities.Tranform
			err := dec.Decode(&obj)

			if err != nil {
				return err
			}

			g.movedObjects = append(g.movedObjects, obj)
		} else {
			err := dec.Decode(&g.movedObjects[i])

			if err != nil {
				return err
			}
		}
	}

	g.movedObjects = g.movedObjects[:movedLen]

	g.tick = tick
	g.sourceClient = sourceId

	return err
}

func (g *GameUnreliableTickPacket) GetTick() uint32 {
	return g.tick
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []gameentities.Tranform {
	return g.movedObjects
}

func (g *GameUnreliableTickPacket) GetSourceClient() uint32 {
	return g.sourceClient
}

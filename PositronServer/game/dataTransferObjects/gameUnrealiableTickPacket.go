package datatransferobjects

import (
	gameentities "positron/game/gameEntities"

	"github.com/vmihailenco/msgpack/v5"
)

type GameUnreliableTickPacket struct {
	_msgpack struct{} `msgpack:",as_array"`

	SourceClient uint32
	MovedObjects []*gameentities.Tranform
}

func NewGameUnreliableTickPacket(movedObjects []*gameentities.Tranform, sourceClient uint32) *GameUnreliableTickPacket {
	return &GameUnreliableTickPacket{
		SourceClient: sourceClient,
		MovedObjects: movedObjects,
	}
}

func (g *GameUnreliableTickPacket) ReassignUnreliableTickPacket(movedObjects []*gameentities.Tranform, sourceClient uint32) {
	g.SourceClient = sourceClient
	g.MovedObjects = movedObjects
}

func (g *GameUnreliableTickPacket) EncodeMsgpack(enc *msgpack.Encoder) error {
	enc.EncodeArrayLen(2)
	err := enc.EncodeUint(uint64(g.SourceClient))

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
	dec.DecodeArrayLen()
	sourceId, err := dec.DecodeUint()

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

	g.SourceClient = uint32(sourceId)
	g.MovedObjects = moved

	return err
}

func (g *GameUnreliableTickPacket) GetMovedObjects() []*gameentities.Tranform {
	return g.MovedObjects
}

func (g *GameUnreliableTickPacket) GetSourceClient() uint32 {
	return g.SourceClient
}

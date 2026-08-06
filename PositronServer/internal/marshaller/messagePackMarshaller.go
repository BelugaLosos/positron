package marshaller

import (
	"bytes"
	"sync"

	"github.com/vmihailenco/msgpack/v5"
)

type MessagePackMarshaller struct{}

var readers = &sync.Pool{
	New: func() any {
		return &bytes.Reader{}
	},
}

func NewMessagePackMarshaller() *MessagePackMarshaller {
	return &MessagePackMarshaller{}
}

func (m *MessagePackMarshaller) Marshal(obj any) ([]byte, error) {
	return msgpack.Marshal(obj)
}

func (m *MessagePackMarshaller) MarshalNonAlloc(buf *bytes.Buffer, obj any) error {
	buf.Reset()
	// TODO: Implement memory bloat protection
	enc := msgpack.GetEncoder()
	enc.UseCompactInts(true)
	enc.UseCompactFloats(true)

	defer msgpack.PutEncoder(enc)
	defer enc.Reset(nil)

	enc.Reset(buf)
	return enc.Encode(obj)
}

func (m *MessagePackMarshaller) Unmarshal(data []byte, obj any) error {
	reader := readers.Get().(*bytes.Reader)
	reader.Reset(data)

	decoder := msgpack.GetDecoder()
	decoder.Reset(reader)

	defer msgpack.PutDecoder(decoder)
	defer decoder.Reset(nil)
	defer readers.Put(reader)
	defer reader.Reset(nil)

	err := decoder.Decode(obj)
	if err != nil {
		return err
	}

	return nil
}

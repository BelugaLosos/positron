package marshaller

import (
	"bytes"
	"fmt"
	"log"

	"github.com/vmihailenco/msgpack/v5"
)

type MessagePackMarshaller struct{}

func NewMessagePackMarshaller() *MessagePackMarshaller {
	return &MessagePackMarshaller{}
}

func (m *MessagePackMarshaller) Marshal(obj any) []byte {
	data, err := msgpack.Marshal(obj)

	if err != nil {
		log.Println(err)
		return []byte{}
	}

	return data
}

func (m *MessagePackMarshaller) Unmarshal(data []byte, obj any) error {
	reader := bytes.NewReader(data)
	decoder := msgpack.NewDecoder(reader)

	err := decoder.Decode(obj)
	if err != nil {
		return fmt.Errorf("failed to unmarshal data: %w", err)
	}

	return nil
}

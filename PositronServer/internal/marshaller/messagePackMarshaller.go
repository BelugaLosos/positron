package marshaller

import (
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
	return msgpack.Unmarshal(data, obj)
}

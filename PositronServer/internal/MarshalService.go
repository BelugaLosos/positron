package internal

import "bytes"

type MarshalService interface {
	Marshal(obj any) ([]byte, error)
	MarshalNonAlloc(buf *bytes.Buffer, obj any) error
	Unmarshal(data []byte, obj any) error
}

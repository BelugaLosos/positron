package internal

type MarshalService interface {
	Marshal(obj any) []byte
	Unmarshal(data []byte, obj any) error
}

package internal

type HandlersFactory interface {
	Create(uuid string) []Handler
}

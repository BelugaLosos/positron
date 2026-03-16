package internal

type HandlersFactory interface {
	Create() ([]Handler, Handler)
}

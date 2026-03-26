package internal

import (
	"sync"
)

type PositronTransportServer interface {
	Start(addr string, handlersFactory HandlersFactory, gServer GameServerAdaper, wg *sync.WaitGroup) error
	Stop() error
	SendToPeer(data []byte, eventType byte, peerUuid string, reliable bool) error
	GetPeerHandlers(peerUuid string) []Handler
	KickClient(uuid string)
}

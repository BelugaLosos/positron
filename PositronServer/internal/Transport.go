package internal

import (
	"sync"
)

type PositronTransportServer interface {
	Start(addr string, handlersFactory HandlersFactory, gServer GameServerAdaper, wg *sync.WaitGroup) error
	Stop() error
	SendToPeer(data []byte, eventType byte, peerUuid string) error
	GetPeerHandlers(peerUuid string) []Handler
}

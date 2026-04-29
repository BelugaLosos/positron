package transport

import (
	"errors"
	"log"
	"net/http"
	"positron/internal"
	"sync"

	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"github.com/pierrec/lz4/v4"
)

type WsTransport struct {
	shutdown chan struct{}

	mutex       *sync.RWMutex
	wg          *sync.WaitGroup
	server      *http.Server
	connections map[string]*wsPeer
	handlers    map[*wsPeer][]internal.Handler

	gServer         internal.GameServerAdaper
	handlersFactory internal.HandlersFactory
}

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
	CheckOrigin:     func(r *http.Request) bool { return true },
}

const defaultBufferSize = 64 * 1024

func NewWsTransport() *WsTransport {
	return &WsTransport{
		shutdown:    make(chan struct{}),
		mutex:       &sync.RWMutex{},
		connections: make(map[string]*wsPeer),
		handlers:    make(map[*wsPeer][]internal.Handler),
	}
}

func (t *WsTransport) Start(addr string, handlersFactory internal.HandlersFactory, gServer internal.GameServerAdaper, wg *sync.WaitGroup) error {
	t.wg = wg
	t.wg.Add(1)
	t.handlersFactory = handlersFactory
	t.gServer = gServer

	mux := http.NewServeMux()
	mux.HandleFunc("/", t.handleUpgrade)

	t.server = &http.Server{
		Addr:    addr,
		Handler: mux,
	}

	go func() {
		log.Printf("WebSocket transport listening on %s...", addr)
		if err := t.server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatalf("Listen error: %s\n", err)
		}
	}()

	return nil
}

func (t *WsTransport) Stop() error {
	defer t.wg.Done()
	close(t.shutdown)

	t.mutex.Lock()
	for _, conn := range t.connections {
		conn.wsConn.Close()
	}
	t.mutex.Unlock()

	return t.server.Close()
}

func (t *WsTransport) SendToPeer(data []byte, eventType byte, peerUuid string, reliable bool) error {
	t.mutex.RLock()
	peer, ok := t.connections[peerUuid]
	t.mutex.RUnlock()

	if !ok || peer == nil {
		return errors.New("peer not found")
	}

	var targetData []byte
	compressionFlag := 0

	peer.mutex.Lock()
	defer peer.mutex.Unlock()

	if len(data) > 1000 {
		if cap(peer.compressionBuf) < lz4.CompressBlockBound(len(data)) {
			tempCompressionBuf := make([]byte, lz4.CompressBlockBound(len(data)))
			compressedSize, compressionErr := lz4.CompressBlock(data, tempCompressionBuf, nil)
			if compressionErr != nil {
				log.Printf("Compression error for peer %s: %v", peerUuid, compressionErr)
				targetData = data
			} else {
				targetData = tempCompressionBuf[:compressedSize]
				compressionFlag = 1
			}
		} else {
			compressedSize, compressionErr := lz4.CompressBlock(data, peer.compressionBuf, nil)
			if compressionErr != nil {
				log.Printf("Compression error for peer %s: %v", peerUuid, compressionErr)
				targetData = data
			} else {
				targetData = peer.compressionBuf[:compressedSize]
				compressionFlag = 1
			}
		}
	} else {
		targetData = data
	}

	totalLen := len(targetData) + 2

	buf := make([]byte, totalLen)
	buf[0] = eventType
	buf[1] = byte(compressionFlag)
	copy(buf[2:], targetData)

	select {
	case peer.send <- buf:
		return nil
	default:
		return errors.New("peer buffer full, packet dropped")
	}
}

func (t *WsTransport) GetPeerHandlers(peerUuid string) []internal.Handler {
	t.mutex.RLock()
	defer t.mutex.RUnlock()

	peer := t.connections[peerUuid]
	if peer == nil {
		return nil
	}
	return t.handlers[peer]
}

func (t *WsTransport) KickClient(uuid string) {
	t.mutex.Lock()
	defer t.mutex.Unlock()

	peer, ok := t.connections[uuid]

	if ok {
		peer.ClosePeer()
	}
}

func (t *WsTransport) handleUpgrade(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("Upgrade error:", err)
		return
	}

	peer := &wsPeer{
		mutex:            &sync.Mutex{},
		send:             make(chan []byte, 1024),
		wsConn:           conn,
		isClosed:         false,
		readBuf:          make([]byte, defaultBufferSize),
		compressionBuf:   make([]byte, defaultBufferSize),
		decompressionBuf: make([]byte, defaultBufferSize),
	}

	id := uuid.New().String()

	t.mutex.Lock()
	handlers, disconnectHandler := t.handlersFactory.Create()
	t.connections[id] = peer
	t.handlers[peer] = handlers
	t.mutex.Unlock()

	for i := range handlers {
		if handlers[i] == nil {
			log.Printf("Handler by id %v is nil of len %v for peer %s", i, len(handlers), id)
			continue
		}
		handlers[i].Init(t, t.gServer, id)
	}

	go peer.sendPump()
	go t.handleIncoming(id, peer, handlers, disconnectHandler)
}

func (t *WsTransport) handleIncoming(id string, peer *wsPeer, handlers []internal.Handler, closeHandler internal.Handler) {
	defer func() {
		peer.ClosePeer()

		t.mutex.Lock()
		delete(t.connections, id)
		delete(t.handlers, peer)
		t.mutex.Unlock()

		closeHandler.PassHandle([]byte{})
	}()

	for {
		select {
		case <-t.shutdown:
			return
		default:
			_, reader, err := peer.wsConn.NextReader()

			if err != nil {
				if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
					log.Printf("WebSocket read error for peer %s: %v", id, err)
				}

				return
			}

			readedAmount, readErr := reader.Read(peer.readBuf)

			if readErr != nil {
				log.Printf("Failed to read from websocket for peer %s: %v", id, readErr)
				return
			}
			packet := peer.readBuf[:readedAmount]

			if len(packet) >= 3 {
				t.handlePacket(handlers, peer, packet)
			} else {
				log.Printf("Received malformed packet (too short) from peer %s, closing connection.", id)
				return
			}
		}
	}
}

func (t *WsTransport) handlePacket(handlers []internal.Handler, peer *wsPeer, packet []byte) {
	usedCompression := false

	if len(packet) > 1 && packet[1] == 1 {
		usedCompression = true
	}

	for i := range handlers {
		if handlers[i] == nil {
			log.Printf("Warning: nil handler found at index %d while processing packet from peer %s", i, peer.wsConn.RemoteAddr().String())
			continue
		}

		if handlers[i].GetType() == packet[0] {
			if usedCompression {
				decompressedLen, err := lz4.UncompressBlock(packet[2:], peer.decompressionBuf)

				if err != nil {
					log.Printf("Decompression error for peer %s: %v", peer.wsConn.RemoteAddr().String(), err)
					continue
				}
				handlers[i].PassHandle(peer.decompressionBuf[:decompressedLen])
			} else {
				handlers[i].PassHandle(packet[2:])
			}

			break
		}
	}
}

type wsPeer struct {
	mutex            *sync.Mutex
	send             chan []byte
	wsConn           *websocket.Conn
	isClosed         bool
	readBuf          []byte
	compressionBuf   []byte
	decompressionBuf []byte
}

func (p *wsPeer) sendPump() {
	corruptions := 0

	for {
		data, ok := <-p.send

		if !ok {
			return
		}

		writer, err := p.wsConn.NextWriter(websocket.BinaryMessage)
		if err != nil {
			log.Printf("Peer %s: Can't get writer, closing peer. Error: %v", p.wsConn.RemoteAddr().String(), err)
			p.ClosePeer()
			return
		}

		_, err = writer.Write(data)
		if err != nil {
			log.Printf("Peer %s: Failed to write data. Error: %v", p.wsConn.RemoteAddr().String(), err)
		}

		err = writer.Close()
		if err != nil {
			log.Printf("Peer %s: Failed to close writer. Error: %v", p.wsConn.RemoteAddr().String(), err)
			corruptions++
			if corruptions > 100 {
				log.Printf("Peer %s disconnected due to massive errors shooting (corruption count: %d)", p.wsConn.RemoteAddr().String(), corruptions)
				p.ClosePeer()
				return
			}
		}
	}
}

func (p *wsPeer) ClosePeer() {
	p.mutex.Lock()
	defer p.mutex.Unlock()

	if p.isClosed {
		return
	}

	if err := p.wsConn.Close(); err != nil {
		log.Printf("Error closing WebSocket connection for peer %s: %v", p.wsConn.RemoteAddr().String(), err)
	}

	close(p.send)
	p.isClosed = true
	log.Printf("Peer %s connection closed.", p.wsConn.RemoteAddr().String())
}

package transport

import (
	"errors"
	"log"
	"net/http"
	"positron/internal"
	"sync"

	"github.com/google/uuid"
	"github.com/gorilla/websocket"
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

var bufferPool = sync.Pool{
	New: func() interface{} {
		return make([]byte, 4096)
	},
}

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

	totalLen := len(data) + 1

	buf := bufferPool.Get().([]byte)
	buf[0] = eventType
	copy(buf[1:], data)

	buf = buf[:totalLen]

	select {
	case peer.send <- buf:
		return nil
	default:
		buf = buf[:cap(buf)]
		bufferPool.Put(buf)
		return errors.New("peer buffer full, packet dropped")
	}
}

func (t *WsTransport) GetPeerHandlers(peerUuid string) []internal.Handler {
	t.mutex.RLock()
	defer t.mutex.RUnlock()

	wsConn := t.connections[peerUuid]
	return t.handlers[wsConn]
}

func (t *WsTransport) handleUpgrade(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	peer := &wsPeer{
		mutex:    &sync.Mutex{},
		send:     make(chan []byte, 1024),
		wsConn:   conn,
		isClosed: false,
	}

	if err != nil {
		log.Println("Upgrade error:", err)
		return
	}

	id := uuid.New().String()

	t.mutex.Lock()

	t.connections[id] = peer
	t.handlers[peer] = t.handlersFactory.Create(id)
	handlers := t.handlers[peer]

	for i := range handlers {
		handlers[i].Init(t, t.gServer, id)
	}

	t.mutex.Unlock()

	go peer.sendPump()
	go t.handleIncoming(id, peer, handlers)
}

func (t *WsTransport) handleIncoming(id string, wsConn *wsPeer, handlers []internal.Handler) {
	defer func() {
		wsConn.ClosePeer()

		t.mutex.Lock()
		delete(t.connections, id)
		delete(t.handlers, wsConn)
		t.mutex.Unlock()
	}()

	for {
		select {
		case <-t.shutdown:
			return
		default:
			_, message, err := wsConn.wsConn.ReadMessage()

			if err != nil {
				if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
					log.Printf("error: %v", err)
				}
				return
			}

			if len(message) >= 3 {
				t.handlePacket(handlers, message)
			} else {
				wsConn.ClosePeer()
			}
		}
	}
}

func (t *WsTransport) handlePacket(handlers []internal.Handler, packet []byte) {
	for i := range handlers {
		if handlers[i].GetType() == packet[0] {
			handlers[i].PassHandle(packet[1:])
		}
	}
}

type wsPeer struct {
	mutex    *sync.Mutex
	send     chan []byte
	wsConn   *websocket.Conn
	isClosed bool
}

func (p *wsPeer) sendPump() {
	corruptions := 0

	for {
		data, ok := <-p.send

		if !ok {
			return
		}

		err := p.wsConn.WriteMessage(websocket.BinaryMessage, data)

		data = data[:cap(data)]
		bufferPool.Put(data)

		if err != nil {
			log.Println(err)
			corruptions++

			if corruptions > 100 {
				log.Println("Peer disconnected in cause of massive errors shooting")

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

	p.wsConn.Close()
	close(p.send)
	p.isClosed = true
}

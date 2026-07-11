package tests

import (
	"log"
	"net/url"
	eventtypes "positron/game/gameHandlers/eventTypes"
	gameserver "positron/game/gameServer"
	"positron/game/room"
	"positron/internal"
	"positron/internal/transport"
	"positron/util"
	"sync"
	"testing"
	"time"

	"github.com/gorilla/websocket"
)

type mockGameServer struct{}

func (m *mockGameServer) GetRoom(roomUuid string) *room.Room {
	return nil
}
func (m *mockGameServer) GetAllRooms() []*room.Room {
	return nil
}
func (m *mockGameServer) CreateRoom(name string, maxSlots int32, ttl time.Duration, scene uint32, tickrate uint32, externalData []byte) string {
	return "nil"
}
func (m *mockGameServer) GetMarshaller() internal.MarshalService {
	return nil
}
func (m *mockGameServer) GetVersion() string {
	return "nil"
}

type wsClientHelper struct {
	shutdown    chan interface{}
	conn        *websocket.Conn
	errChan     chan error
	dataReceive chan *wsPacket
}

func (w *wsClientHelper) connect(addr string, t *testing.T) error {
	url := url.URL{Scheme: "ws", Host: addr, Path: "/"}
	conn, _, err := websocket.DefaultDialer.Dial(url.String(), nil)

	if err != nil {
		return err
	}

	w.shutdown = make(chan interface{})
	w.conn = conn
	w.dataReceive = make(chan *wsPacket, 1024)
	w.errChan = make(chan error, 1024)

	go w.reader()
	go w.testingChannelReader(t)

	return nil
}

func (w *wsClientHelper) disconnect() error {
	close(w.shutdown)
	return w.conn.Close()
}

func (w *wsClientHelper) reader() {
	for {
		select {
		case <-w.shutdown:
			return
		default:
			_, data, err := w.conn.ReadMessage()

			if err != nil {
				w.errChan <- err
				continue
			}

			packet := &wsPacket{}
			packet.event, packet.wasCompressed, packet.originalDataSize, packet.rawPayload = util.DeconstructPacket(data)

			w.dataReceive <- packet
		}
	}
}

func (w *wsClientHelper) testingChannelReader(t *testing.T) {
	for {
		select {
		case <-w.shutdown:
			return
		case err := <-w.errChan:
			t.Error(err)
		}
	}
}

type wsPacket struct {
	event            byte
	wasCompressed    bool
	originalDataSize uint32
	rawPayload       []byte
}

func (w *wsClientHelper) writeSync(event byte, compressed bool, data []byte) error { // this method is NOT concurent/thread safe!
	return w.conn.WriteMessage(websocket.BinaryMessage, util.GlueDataToOptions(event, compressed, uint32(len(data)), data))
}

func TestStartAndStop(t *testing.T) {
	wg := &sync.WaitGroup{}
	startTime := time.Now()

	transport := transport.NewWsTransport()
	err := transport.Start("127.0.0.1:12345", gameserver.NewGameHandlersFactory(&mockGameServer{}), &mockGameServer{}, wg)

	if err != nil {
		t.Error(err)
	}

	go func() {
		time.Sleep(time.Second)
		serr := transport.Stop()

		if serr != nil {
			t.Error(serr)
		}
	}()

	wg.Wait()

	elapsed := (time.Since(startTime) - time.Second)
	if elapsed > 25*time.Millisecond {
		t.Errorf("Incurate timings %v", time.Since(startTime))
	}
}

func TestConnectMockedClient(t *testing.T) {
	wg := &sync.WaitGroup{}
	addr := "127.0.0.1:12345"

	transport := transport.NewWsTransport()
	err := transport.Start(addr, gameserver.NewGameHandlersFactory(&mockGameServer{}), &mockGameServer{}, wg)

	if err != nil {
		t.Error(err)
	}

	time.Sleep(150 * time.Millisecond) // wait for starting server

	wsClient := &wsClientHelper{}
	cerr := wsClient.connect(addr, t)

	if cerr != nil {
		t.Error(cerr)
	}

	time.Sleep(150 * time.Millisecond)

	werr := wsClient.writeSync(eventtypes.VERSION_CHECK_REQUEST, false, []byte("nil"))

	if werr != nil {
		t.Error(werr)
	}

	go func() {
		time.Sleep(time.Second)
		log.Println("Closing all connections")

		response := (<-wsClient.dataReceive).rawPayload

		if len(response) != 1 || response[0] != 1 {
			t.Errorf("Invalid version check response: %v", response)
		}

		derr := wsClient.disconnect()

		if derr != nil {
			t.Error(derr)
		}

		time.Sleep(500 * time.Millisecond)

		serr := transport.Stop()

		if serr != nil {
			t.Error(serr)
		}
	}()

	wg.Wait()
}

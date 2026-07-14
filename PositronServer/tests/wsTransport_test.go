package tests

import (
	"fmt"
	"log"
	"math/rand/v2"
	"net/url"
	eventtypes "positron/game/gameHandlers/eventTypes"
	gameserver "positron/game/gameServer"
	"positron/game/room"
	"positron/internal"
	"positron/internal/transport"
	"positron/util"
	"strconv"
	"strings"
	"sync"
	"testing"
	"time"

	"github.com/gorilla/websocket"
	"github.com/pierrec/lz4/v4"
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
	dataSend    chan *wsPacket
}

func (w *wsClientHelper) connect(addr string, t *testing.T) error {
	url := url.URL{Scheme: "ws", Host: addr, Path: "/"}

	dialer := websocket.Dialer{
		WriteBufferSize: 256 * 1024,
		ReadBufferSize:  256 * 1024,
	}

	conn, _, err := dialer.Dial(url.String(), nil)

	if err != nil {
		return err
	}

	w.shutdown = make(chan interface{})
	w.conn = conn
	w.dataReceive = make(chan *wsPacket, 3024)
	w.errChan = make(chan error, 3024)
	w.dataSend = make(chan *wsPacket, 3024)

	go w.reader()
	go w.writer()
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
			close(w.dataReceive)
			return
		default:
			_, data, err := w.conn.ReadMessage()

			if err != nil {
				if websocket.IsUnexpectedCloseError(err) {
					w.disconnect()
					return
				}

				w.errChan <- err
				return
			}

			packet := &wsPacket{}
			packet.event, packet.wasCompressed, packet.originalDataSize, packet.rawPayload = util.DeconstructPacket(data)

			if packet.wasCompressed {
				decompressBuffer := make([]byte, 250_000)
				decompressedLen, err := lz4.UncompressBlock(packet.rawPayload, decompressBuffer)

				if err != nil {
					w.errChan <- err
					return
				}

				packet.rawPayload = decompressBuffer[:decompressedLen]
			}

			w.dataReceive <- packet
		}
	}
}

func (w *wsClientHelper) writer() {
	for {
		select {
		case <-w.shutdown:
			close(w.dataSend)
			return
		default:
			packet := <-w.dataSend
			err := w.conn.WriteMessage(websocket.BinaryMessage, util.GlueDataToOptions(packet.event, packet.wasCompressed, packet.originalDataSize, packet.rawPayload))

			if err != nil {
				w.errChan <- err
				continue
			}
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

func (w *wsClientHelper) write(event byte, compressed bool, data []byte) {
	w.dataSend <- &wsPacket{
		event:            event,
		wasCompressed:    compressed,
		originalDataSize: uint32(len(data)),
		rawPayload:       data,
	}
}

type echoHandler struct {
	transport internal.PositronTransportServer
	connUuid  string
}

func (e *echoHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	e.transport = transport
	e.connUuid = connectionUuid
}

func (e *echoHandler) GetType() byte {
	return 0x0
}
func (e *echoHandler) PassHandle(packet []byte) {
	if err := e.transport.SendToPeer(packet, 0x3, e.connUuid, true); err != nil {
		log.Println(err)
	}
}

func (e *echoHandler) SetRoom(room *room.Room, inRoomId uint32) {
}

type uuidReturnHandler struct {
	transport internal.PositronTransportServer
	connUuid  string
}

func (e *uuidReturnHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	e.transport = transport
	e.connUuid = connectionUuid
}

func (e *uuidReturnHandler) GetType() byte {
	return 0x1
}
func (e *uuidReturnHandler) PassHandle(packet []byte) {
	if err := e.transport.SendToPeer([]byte(e.connUuid), 0x2, e.connUuid, true); err != nil {
		log.Fatal(err)
	}
}

func (e *uuidReturnHandler) SetRoom(room *room.Room, inRoomId uint32) {
}

type mockHandlersFactory struct{}

func (m *mockHandlersFactory) Create() ([]internal.Handler, internal.Handler) {
	handlers := make([]internal.Handler, 0)
	handlers = append(handlers, &echoHandler{})
	handlers = append(handlers, &uuidReturnHandler{})

	return handlers, handlers[0]
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

	wsClient.write(eventtypes.VERSION_CHECK_REQUEST, false, []byte("nil"))

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

func TestKick(t *testing.T) {
	wg := &sync.WaitGroup{}
	addr := "127.0.0.1:12345"

	transport := transport.NewWsTransport()
	err := transport.Start(addr, &mockHandlersFactory{}, &mockGameServer{}, wg)

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

	wsClient.write(0x1, false, []byte("nil"))

	uuid := string((<-wsClient.dataReceive).rawPayload)
	transport.KickClient(uuid)

	go func() {
		time.Sleep(time.Second)
		log.Println("Closing all connections")

		if concurrentConnectionsCount := transport.GetCurrentConnectedPeersCount(); concurrentConnectionsCount != 0 {
			t.Error("kicked client does not appeared as kicked and still in the active connections list")
		}

		time.Sleep(500 * time.Millisecond)

		serr := transport.Stop()

		if serr != nil {
			t.Error(serr)
		}
	}()

	wg.Wait()
}

func TestConcurrentTrafficForCorruption(t *testing.T) {
	wg := &sync.WaitGroup{}
	addr := "127.0.0.1:12345"

	transport := transport.NewWsTransport()
	if err := transport.Start(addr, &mockHandlersFactory{}, &mockGameServer{}, wg); err != nil {
		t.Fatal(err)
	}

	defer func() {
		transport.Stop()
		wg.Wait()
	}()

	time.Sleep(50 * time.Millisecond)

	wsClient := &wsClientHelper{}
	if err := wsClient.connect(addr, t); err != nil {
		t.Fatal(err)
	}
	defer wsClient.disconnect()

	origin := "This string is data that would be fragmented and transferred over loopback network using 10 concurrent tasks and one channel E"

	type Chunk struct {
		Index int
		Value string
	}

	inputChan := make(chan Chunk, len(origin))
	for i, char := range origin {
		inputChan <- Chunk{Index: i, Value: string(char)}
	}
	close(inputChan)

	sendingDataChanWg := sync.WaitGroup{}
	numWorkers := 10
	sendingDataChanWg.Add(numWorkers)

	for i := 0; i < numWorkers; i++ {
		go func() {
			defer sendingDataChanWg.Done()
			for chunk := range inputChan {
				payload := fmt.Sprintf("%d|%s", chunk.Index, chunk.Value)
				wsClient.write(0x0, false, []byte(payload))
			}
		}()
	}

	sendingDataChanWg.Wait()

	receivedChunks := make([]string, len(origin))
	timeout := time.After(2 * time.Second)
	chunksCollected := 0

	for chunksCollected < len(origin) {
		select {
		case msg := <-wsClient.dataReceive:
			strPayload := string(msg.rawPayload)
			parts := strings.SplitN(strPayload, "|", 2)
			if len(parts) != 2 {
				t.Fatalf("Corrupted frame protocol received: %s", strPayload)
			}

			idx, err := strconv.Atoi(parts[0])
			if err != nil {
				t.Fatalf("Failed to parse sequence index: %v", err)
			}
			charValue := parts[1]

			receivedChunks[idx] = charValue
			chunksCollected++

		case <-timeout:
			t.Fatalf("Timeout reached. Gathered %d/%d chunks.", chunksCollected, len(origin))
		}
	}

	received := strings.Join(receivedChunks, "")

	if received != origin {
		t.Errorf("Data corruption detected!\nExpected: %s\nReceived: %s", origin, received)
	} else {
		log.Println("Success: Structural data concurrency matches fully without loss.")
	}
}

func TestConcurrentDataCorruption(t *testing.T) {
	wg := &sync.WaitGroup{}
	addr := "127.0.0.1:12345"

	transport := transport.NewWsTransport()
	if err := transport.Start(addr, &mockHandlersFactory{}, &mockGameServer{}, wg); err != nil {
		t.Fatal(err)
	}

	defer func() {
		transport.Stop()
		wg.Wait()
	}()

	time.Sleep(50 * time.Millisecond)

	wsClient := &wsClientHelper{}
	if err := wsClient.connect(addr, t); err != nil {
		t.Fatal(err)
	}
	defer wsClient.disconnect()

	expectationMap := make(map[int]string)

	for i := range 25 {
		expectationMap[i] = generateRandomString(30_001)
	}

	numWorkers := 1000 // this may be limited by send channel size. if processor not rapid enought and can`t pop data from this channel rapidly it may drop packets
	workersWg := &sync.WaitGroup{}

	workersWg.Add(numWorkers)
	for range numWorkers {
		go func() {
			defer workersWg.Done()

			index := rand.IntN(len(expectationMap))
			wsClient.write(0x0, false, []byte(strconv.Itoa(index)+"#"+expectationMap[index]))
		}()
	}
	workersWg.Wait()

	receivedMessages := 0

	for {
		if receivedMessages == numWorkers {
			break
		}

		packet, ok := <-wsClient.dataReceive
		receivedMessages++

		if !ok {
			t.Error("channel error")
			return
		}

		log.Println(receivedMessages)

		data := string(packet.rawPayload)
		key, _ := strconv.Atoi(strings.Split(data, "#")[0])
		value := strings.Split(data, "#")[1]

		log.Printf("is compressed %v size %v", packet.wasCompressed, len(value))

		if mapValue, exists := expectationMap[key]; !exists || mapValue != value {
			t.Errorf("corruption. \nexistance %v \nval %s \nval_len %v \nrec %s \nlen_rec %v", exists, mapValue, len(mapValue), value, len(value))
		}
	}

	log.Println("Ok")
}

func TestMultipleConcurrentConnections(t *testing.T) {
	wg := &sync.WaitGroup{}
	addr := "127.0.0.1:12345"

	transport := transport.NewWsTransport()
	if err := transport.Start(addr, &mockHandlersFactory{}, &mockGameServer{}, wg); err != nil {
		t.Fatal(err)
	}

	defer func() {
		transport.Stop()
		wg.Wait()
	}()

	time.Sleep(50 * time.Millisecond)

	workersWg := &sync.WaitGroup{}
	worketsCount := 512
	messagesPerWorkerAmount := 1000
	messageSize := 4096

	workersWg.Add(worketsCount)

	for i := range worketsCount {
		time.Sleep(10 * time.Millisecond) // preventing windows from blowing up kernel tcp stack. unnecessary on linux

		go func() {
			defer workersWg.Done()

			wsClient := &wsClientHelper{}
			cerr := wsClient.connect(addr, t)

			if cerr != nil {
				t.Errorf("connection err %v current connected %v", cerr, i+1)
				return
			}

			randomData := generateRandomString(messageSize)
			receivedAmount := 0

			for range messagesPerWorkerAmount {
				wsClient.write(0x0, false, []byte(randomData))
			}

			for {
				message := <-wsClient.dataReceive

				if string(message.rawPayload) != randomData {
					t.Error("data corruption")
					return
				} else {
					receivedAmount++

					if receivedAmount == messagesPerWorkerAmount {
						break
					}
				}
			}
		}()
	}

	workersWg.Wait()
}

func generateRandomString(length int) string {
	now := time.Now().UnixNano()
	pcgSource := rand.NewPCG(uint64(now), uint64(now>>32))
	r := rand.New(pcgSource)

	var sb strings.Builder
	sb.Grow(length)
	charset := "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

	for i := 0; i < length; i++ {
		randomIndex := r.IntN(len(charset))
		sb.WriteByte(charset[randomIndex])
	}

	return sb.String()
}

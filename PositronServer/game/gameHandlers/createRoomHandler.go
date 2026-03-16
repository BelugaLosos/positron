package gamehandlers

import (
	"log"
	datatransferobjects "positron/game/gameHandlers/dataTransferObjects"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/game/room"
	"positron/internal"
	"time"
)

type CreateRoomHandler struct {
	transport internal.PositronTransportServer
	uuid      string
	gameSaver internal.GameServerAdaper
	inRoom    bool
}

func NewCreateRoomHandler() *CreateRoomHandler {
	return &CreateRoomHandler{}
}

func (c *CreateRoomHandler) Init(transport internal.PositronTransportServer, gServer internal.GameServerAdaper, connectionUuid string) {
	c.transport = transport
	c.uuid = connectionUuid
	c.gameSaver = gServer
}

func (c *CreateRoomHandler) GetType() byte {
	return eventtypes.CREATE_ROOM
}

func (c *CreateRoomHandler) PassHandle(packet []byte) {
	if c.inRoom {
		return
	}

	var data datatransferobjects.CreateRoomPacket
	err := c.gameSaver.GetMarshaller().Unmarshal(packet, data)

	if err != nil {
		log.Println(err)
		return
	}

	uuid := c.gameSaver.CreateRoom(data.GetName(), int(data.GetPlayerCap()), 10*time.Second)

	response := datatransferobjects.NewRoomCreationResponsePacket(uuid)
	binResponse, _ := c.gameSaver.GetMarshaller().Marshal(response)

	err = c.transport.SendToPeer(binResponse, eventtypes.ROOM_CREATED, c.uuid, true)

	if err != nil {
		log.Println(err)
	}
}

func (c *CreateRoomHandler) SetRoom(room *room.Room, inRoomId uint32) {
	if room != nil {
		c.inRoom = true
	} else {
		c.inRoom = false
	}
}

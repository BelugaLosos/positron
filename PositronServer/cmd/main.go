package main

import (
	"log"
	"net/http"
	_ "net/http/pprof"
	gameserver "positron/game/gameServer"
	"positron/internal/marshaller"
	"positron/internal/transport"
	"sync"
)

func main() {
	go func() {
		http.ListenAndServe("localhost:6060", nil)
	}()

	wg := &sync.WaitGroup{}
	game := gameserver.NewGameServer("127.0.0.1:7070", transport.NewWsTransport(), marshaller.NewMessagePackMarshaller())

	err := game.Start(wg)
	defer stop(game)

	if err != nil {
		panic(err)
	}

	wg.Wait()
}

func stop(gServer *gameserver.GameServer) {
	err := gServer.Stop()

	if err != nil {
		log.Println(err)
	} else {
		log.Println("Stopped succesfully !")
	}
}

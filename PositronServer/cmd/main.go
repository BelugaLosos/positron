package main

import (
	"flag"
	"log"
	"net/http"
	_ "net/http/pprof"
	gameserver "positron/game/gameServer"
	"positron/internal/marshaller"
	"positron/internal/transport"
	"strconv"
	"sync"
)

func main() {
	dbgPort := flag.Int("dp", 6060, "pprof debug port on localhosh:dp")
	useDbg := flag.Bool("dbg", false, "if set true program starts pprof on -dp port")
	transportAddr := flag.String("taddr", "127.0.0.1", "main gaming server listening IP address for transport")
	transportPort := flag.Int("tp", 7070, "main port for gaming server")
	controllPort := flag.Int("cp", 7071, "port for controll the server (stop ...)")
	allowStop := flag.Bool("als", true, "allows /term listening")
	version := flag.String("v", "0.0.1 -- DEFAULT", "server version for filtering incoming client connections and prevent version-dependent bugs")
	flag.Parse()

	if *useDbg {
		go func() {
			http.ListenAndServe("localhost:"+strconv.Itoa(*dbgPort), nil)
		}()
	}

	wg := &sync.WaitGroup{}
	game := gameserver.NewGameServer(*transportAddr+":"+strconv.Itoa(*transportPort), transport.NewWsTransport(), marshaller.NewMessagePackMarshaller(), *version)

	log.Printf("Starting positron semi-dedicated server v%s", *version)
	err := game.Start(wg)

	if err != nil {
		panic(err)
	}

	go func() {
		if *allowStop {
			http.HandleFunc("/term", func(w http.ResponseWriter, r *http.Request) {
				w.Write([]byte("OK"))
				stop(game, wg)
			})
		}

		http.ListenAndServe("localhost:"+strconv.Itoa(*controllPort), nil)
	}()

	wg.Wait()
}

func stop(gServer *gameserver.GameServer, wg *sync.WaitGroup) {
	wg.Add(1)
	err := gServer.Stop()

	if err != nil {
		log.Println(err)
	} else {
		log.Println("Stopped succesfully !")
	}

	wg.Done()
}

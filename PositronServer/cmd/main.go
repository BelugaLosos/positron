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
	ticksToRetransmitStaticObjects := flag.Uint("rtlim", 50, "This value means how much static object`s positions (UNR CHAN) will be retransmitted over the network to prevent desync over UNR CHAN (Unreliable transport channel)")
	ticksToMarkObjectAsStatic := flag.Uint("rtthr", 150, "This is ticks amount to mark position as static, any object on scene has specific counter that resets while processing move")
	forceDisableStaticsRetransmit := flag.Bool("drt", false, "If set true forces server to ignore scoring objects and retransmit statics")

	flag.Parse()

	if *useDbg {
		go func() {
			http.ListenAndServe("localhost:"+strconv.Itoa(*dbgPort), nil)
		}()
	}

	wg := &sync.WaitGroup{}
	game := gameserver.NewGameServer(*transportAddr+":"+strconv.Itoa(*transportPort), transport.NewWsTransport(), marshaller.NewMessagePackMarshaller(), *version, int(*ticksToRetransmitStaticObjects), int(*ticksToMarkObjectAsStatic), bool(*forceDisableStaticsRetransmit))

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

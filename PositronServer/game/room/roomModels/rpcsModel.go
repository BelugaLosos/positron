package roommodels

import (
	"log"
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/internal/arena"
	"sync"
)

type RpcsModel struct {
	mutex *sync.Mutex

	cachedRpcs               []gameentities.RpcCall
	cachedRpcsTransientArena *arena.TransientArena

	callBuffer               []gameentities.RpcCall
	callBufferTransientArena *arena.TransientArena

	incomingCallBufferTransientArena *arena.TransientArena
}

func NewRpcsModel() *RpcsModel {
	return &RpcsModel{
		mutex:                            &sync.Mutex{},
		cachedRpcs:                       make([]gameentities.RpcCall, 0, 16),
		cachedRpcsTransientArena:         arena.NewTransfientArena(),
		callBuffer:                       make([]gameentities.RpcCall, 0, 16),
		callBufferTransientArena:         arena.NewTransfientArena(),
		incomingCallBufferTransientArena: arena.NewTransfientArena(),
	}
}

func (r *RpcsModel) GetCachedRpcs() ([]gameentities.RpcCall, []byte) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	return r.cachedRpcs, r.cachedRpcsTransientArena.ReadAll()
}

func (r *RpcsModel) GetCurrentCallBuffer() ([]gameentities.RpcCall, []byte) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	return r.callBuffer, r.callBufferTransientArena.ReadAll()
}

func (r *RpcsModel) ResetTempBuffers() {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.callBufferTransientArena.Flush()
	r.callBuffer = r.callBuffer[:0]
}

func (r *RpcsModel) PutTransientDataIncoming(data []byte) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	r.incomingCallBufferTransientArena.Flush()
	r.incomingCallBufferTransientArena.CloneFrom(data)
}

func (r *RpcsModel) Call(call gameentities.RpcCall, gameObjectsAddMod []gameentities.GameObject) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	incomingPayload, err := r.incomingCallBufferTransientArena.Read(call.GetDescriptors())

	if err != nil {
		log.Printf("Unable to call rpc due to memory error %v", err)
		return
	}

	incominfDataLen := uint32(len(incomingPayload))

	isCallForFreshObject, creationId := gameentities.TryGetCreationIdRpc(incomingPayload)

	if isCallForFreshObject {
		for i := range gameObjectsAddMod {
			if gameObjectsAddMod[i].GetCreationId() == creationId {
				call.SetObjectId(gameObjectsAddMod[i].GetId())

				break
			}
		}
	}

	deltaPtr := r.callBufferTransientArena.Alloc(incomingPayload)
	call.SetDescriptors(deltaPtr, incominfDataLen)

	r.callBuffer = append(r.callBuffer, call)

	target := call.GetRpcType()

	if target == eventtypes.RPC_ALL_CACHED || target == eventtypes.RPC_OTHERS_CACHED || target == eventtypes.RPC_TARGET_CACHED {
		cachePtr := r.cachedRpcsTransientArena.Alloc(incomingPayload)
		call.SetDescriptors(cachePtr, incominfDataLen)

		r.cachedRpcs = append(r.cachedRpcs, call)
	}
}

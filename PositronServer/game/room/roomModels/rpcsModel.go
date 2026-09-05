package roommodels

import (
	"log"
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"positron/internal/arena"
)

type RpcsModel struct {
	cachedRpcs               []gameentities.RpcCall
	cachedRpcsTransientArena *arena.TransientArena

	callBuffer               []gameentities.RpcCall
	callBufferTransientArena *arena.TransientArena

	incomingCallBufferTransientArena *arena.TransientArena
}

func NewRpcsModel() *RpcsModel {
	return &RpcsModel{
		cachedRpcs:                       make([]gameentities.RpcCall, 0, 16),
		cachedRpcsTransientArena:         arena.NewTransfientArena(),
		callBuffer:                       make([]gameentities.RpcCall, 0, 16),
		callBufferTransientArena:         arena.NewTransfientArena(),
		incomingCallBufferTransientArena: arena.NewTransfientArena(),
	}
}

func (r *RpcsModel) GetCachedRpcs() ([]gameentities.RpcCall, []byte) {
	return r.cachedRpcs, r.cachedRpcsTransientArena.ReadAll()
}

func (r *RpcsModel) GetCurrentCallBuffer() ([]gameentities.RpcCall, []byte) {
	return r.callBuffer, r.callBufferTransientArena.ReadAll()
}

func (r *RpcsModel) ResetTempBuffers() {
	r.callBufferTransientArena.Flush()
	r.callBuffer = r.callBuffer[:0]
}

func (r *RpcsModel) PutTransientDataIncoming(data []byte) {
	r.incomingCallBufferTransientArena.Flush()
	r.incomingCallBufferTransientArena.CloneFrom(data)
}

func (r *RpcsModel) Call(call gameentities.RpcCall, gameObjectsAddMod []gameentities.GameObject) {
	incomingRawPayload, err := r.incomingCallBufferTransientArena.Read(call.GetDescriptors())

	if err != nil {
		log.Printf("Unable to call rpc due to memory error %v", err)
		return
	}

	isCallForFreshObject, creationId, payload := gameentities.DeconstructRpc(incomingRawPayload)
	incomingDataLen := uint32(len(payload))

	if isCallForFreshObject {
		for i := range gameObjectsAddMod {
			if gameObjectsAddMod[i].GetCreationId() == creationId {
				call.SetObjectId(gameObjectsAddMod[i].GetId())

				break
			}
		}
	}

	deltaPtr := r.callBufferTransientArena.Alloc(payload)
	call.SetDescriptors(deltaPtr, incomingDataLen)

	r.callBuffer = append(r.callBuffer, call)

	target := call.GetRpcType()

	if target == eventtypes.RPC_ALL_CACHED || target == eventtypes.RPC_OTHERS_CACHED || target == eventtypes.RPC_TARGET_CACHED {
		cachePtr := r.cachedRpcsTransientArena.Alloc(payload)
		call.SetDescriptors(cachePtr, incomingDataLen)

		r.cachedRpcs = append(r.cachedRpcs, call)
	}
}

func (r *RpcsModel) SanetizeBufferedCalls(objId uint32) {
	for i := range r.cachedRpcs {
		rpc := r.cachedRpcs[i]

		if rpc.GetObjectId() == objId {
			rpc.MarkRpcInvalid()
			r.cachedRpcs[i] = rpc
		}
	}
}

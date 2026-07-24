package roommodels

import (
	gameentities "positron/game/gameEntities"
	eventtypes "positron/game/gameHandlers/eventTypes"
	"sync"
)

type RpcsModel struct {
	mutex *sync.Mutex

	cachedRpcs []gameentities.RpcCall
	callBuffer []gameentities.RpcCall
}

func NewRpcsModel() *RpcsModel {
	return &RpcsModel{
		mutex:      &sync.Mutex{},
		cachedRpcs: make([]gameentities.RpcCall, 0, 16),
		callBuffer: make([]gameentities.RpcCall, 0, 16),
	}
}

func (r *RpcsModel) GetCachedRpcs() []gameentities.RpcCall {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	return r.cachedRpcs
}

func (r *RpcsModel) GetCurrentCallBuffer() []gameentities.RpcCall {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	return r.callBuffer
}

func (r *RpcsModel) ResetTempBuffers() {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	clear(r.callBuffer)
	r.callBuffer = r.callBuffer[:0]
}

func (r *RpcsModel) Call(call gameentities.RpcCall, gameObjectsAddMod []gameentities.GameObject) {
	r.mutex.Lock()
	defer r.mutex.Unlock()

	isCallForFreshObject, creationId := call.TryGetCreationId()

	if isCallForFreshObject {
		for i := range gameObjectsAddMod {
			if gameObjectsAddMod[i].GetCreationId() == creationId {
				call.SetObjectId(gameObjectsAddMod[i].GetId())

				break
			}
		}
	}

	r.callBuffer = append(r.callBuffer, call)
	target := call.GetRpcType()

	if target == eventtypes.RPC_ALL_CACHED || target == eventtypes.RPC_OTHERS_CACHED || target == eventtypes.RPC_TARGET_CACHED {
		r.cachedRpcs = append(r.cachedRpcs, call)
	}
}

package roommodels

import gameentities "positron/game/gameEntities"

type RpcsModel struct {
	cachedRpcs []*gameentities.RpcCall
}

func NewRpcsModel() *RpcsModel {
	return &RpcsModel{
		cachedRpcs: make([]*gameentities.RpcCall, 0),
	}
}

func (r *RpcsModel) GetCachedRpcs() []*gameentities.RpcCall {
	return r.cachedRpcs
}

func (r *RpcsModel) ResetTempBuffers() {

}

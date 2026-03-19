package roommodels

import gameentities "positron/game/gameEntities"

type NetValuesModel struct {
	netValues []*gameentities.NetValue
}

func NewNetValuesModel() *NetValuesModel {
	return &NetValuesModel{
		netValues: make([]*gameentities.NetValue, 0),
	}
}

func (n *NetValuesModel) GetValues() []*gameentities.NetValue {
	return n.netValues
}

func (n *NetValuesModel) ResetTempBuffers() {

}

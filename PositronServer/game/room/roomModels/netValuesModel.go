package roommodels

import (
	gameentities "positron/game/gameEntities"
	"strconv"
	"sync"
)

type NetValuesModel struct {
	mutex *sync.Mutex

	searchMap map[string]*gameentities.NetValue

	netValues              []*gameentities.NetValue
	tempModificationBuffer []*gameentities.NetValue
}

func NewNetValuesModel() *NetValuesModel {
	return &NetValuesModel{
		mutex:                  &sync.Mutex{},
		searchMap:              make(map[string]*gameentities.NetValue),
		netValues:              make([]*gameentities.NetValue, 0),
		tempModificationBuffer: make([]*gameentities.NetValue, 0),
	}
}

func (n *NetValuesModel) GetValues() []*gameentities.NetValue {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	return n.netValues
}

func (n *NetValuesModel) GetTempMod() []*gameentities.NetValue {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	return n.tempModificationBuffer
}

func (n *NetValuesModel) ResetTempBuffers() {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	clear(n.tempModificationBuffer)
	n.tempModificationBuffer = n.tempModificationBuffer[:0]
}

func (n *NetValuesModel) AddOrModify(value *gameentities.NetValue) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	gettenValue, isExist := n.searchMap[n.getKeyOfValue(value)]

	if isExist && gettenValue.GetIsDeleting() {
		return
	}

	if isExist {
		n.modifyValue(value, gettenValue)
	} else {
		n.addValue(value)
	}
}

func (n *NetValuesModel) RemoveAllValuesFromObject(objectUuid uint32) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	for i := range n.netValues {
		val := n.netValues[i]

		if val.GetParentObjectId() == objectUuid {
			delete(n.searchMap, n.getKeyOfValue(val))
			val.MarkAsDeleting()

			n.tempModificationBuffer = append(n.tempModificationBuffer, val)
		}
	}

	clear(n.netValues)
	n.netValues = n.netValues[:0]

	for _, val := range n.searchMap {
		n.netValues = append(n.netValues, val)
	}
}

func (n *NetValuesModel) addValue(value *gameentities.NetValue) {
	n.netValues = append(n.netValues, value)
	n.searchMap[n.getKeyOfValue(value)] = value

	n.tempModificationBuffer = append(n.tempModificationBuffer, value)
}

func (n *NetValuesModel) modifyValue(value *gameentities.NetValue, currentValue *gameentities.NetValue) {
	currentValue.ModifyPayload(value.GetPayload())

	n.tempModificationBuffer = append(n.tempModificationBuffer, currentValue)
}

func (n *NetValuesModel) getKeyOfValue(value *gameentities.NetValue) string {
	vid := strconv.FormatUint(uint64(value.GetValueId()), 10)
	oid := strconv.FormatUint(uint64(value.GetParentObjectId()), 10)

	return oid + vid
}

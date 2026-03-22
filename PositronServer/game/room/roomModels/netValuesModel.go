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

	n.tempModificationBuffer = n.tempModificationBuffer[:0]
}

func (n *NetValuesModel) AddOrModify(value *gameentities.NetValue) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	gettenValue, isExist := n.searchMap[n.getKeyOfValue(value)]

	if isExist && value.GetIsDeleting() {
		return
	}

	if isExist {
		n.modifyValue(value, gettenValue)
	} else {
		n.addValue(value)
	}
}

// need to be called from room when destroying objects
// need to be modified for possibility to remove values from object and all subs it obly wayt to remove value from code
func (n *NetValuesModel) TryRemove(value *gameentities.NetValue) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	isExist, _, index := n.findValue(value)

	if isExist {
		n.netValues[index] = nil
		n.netValues[index] = n.netValues[0]
		n.netValues = n.netValues[1:]

		value.MarkAsDeleting()
		n.tempModificationBuffer = append(n.tempModificationBuffer, value)

		delete(n.searchMap, n.getKeyOfValue(value))
	}
}

func (n *NetValuesModel) findValue(value *gameentities.NetValue) (bool, *gameentities.NetValue, int) {
	for i := range n.netValues {
		currentValue := n.netValues[i]

		if currentValue.GetValueId() == value.GetValueId() && currentValue.GetParentObjectId() == value.GetParentObjectId() && currentValue.GetSubObjectId() == value.GetSubObjectId() {
			return true, currentValue, i
		}
	}

	return false, nil, 0
}

func (n *NetValuesModel) addValue(value *gameentities.NetValue) {
	n.netValues = append(n.netValues, value)
	n.searchMap[n.getKeyOfValue(value)] = value
}

func (n *NetValuesModel) modifyValue(value *gameentities.NetValue, currentValue *gameentities.NetValue) {
	currentValue.ModifyPayload(value.GetPayload())
}

func (n *NetValuesModel) getKeyOfValue(value *gameentities.NetValue) string {
	left := strconv.FormatUint(uint64(value.GetValueId()), 10)
	mid := strconv.FormatUint(uint64(value.GetParentObjectId()), 10)
	right := strconv.FormatUint(uint64(value.GetSubObjectId()), 10)

	return left + mid + right
}

package roommodels

import (
	gameentities "positron/game/gameEntities"
	"sync"
)

type NetValuesModel struct {
	mutex *sync.Mutex

	worldManagedContainer map[uint64]gameentities.NetValue

	worldCache        []gameentities.NetValue
	modificationCache []gameentities.NetValue
}

func NewNetValuesModel() *NetValuesModel {
	return &NetValuesModel{
		mutex:                 &sync.Mutex{},
		worldManagedContainer: make(map[uint64]gameentities.NetValue),
		worldCache:            make([]gameentities.NetValue, 0, 16),
		modificationCache:     make([]gameentities.NetValue, 0, 16),
	}
}

func (n *NetValuesModel) GetValues() []gameentities.NetValue {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	n.rebuildWorldCache()
	return n.worldCache
}

func (n *NetValuesModel) GetTempMod() []gameentities.NetValue {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	return n.modificationCache
}

func (n *NetValuesModel) ResetTempBuffers() {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	n.modificationCache = n.modificationCache[:0]
}

func (n *NetValuesModel) AddOrModify(value gameentities.NetValue) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	gettenValue, isExist := n.worldManagedContainer[n.getKeyOfValue(value)]

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

	for key, val := range n.worldManagedContainer {
		if val.GetParentObjectId() == objectUuid {
			delete(n.worldManagedContainer, key)

			val.MarkAsDeleting()
			n.modificationCache = append(n.modificationCache, val)
		}
	}
}

func (n *NetValuesModel) addValue(value gameentities.NetValue) {
	n.worldManagedContainer[n.getKeyOfValue(value)] = value
	n.modificationCache = append(n.modificationCache, value)
}

func (n *NetValuesModel) modifyValue(value gameentities.NetValue, currentValue gameentities.NetValue) {
	currentValue.ModifyPayload(value.GetPayload())

	n.modificationCache = append(n.modificationCache, currentValue)
	n.worldManagedContainer[n.getKeyOfValue(currentValue)] = currentValue
}

func (n *NetValuesModel) getKeyOfValue(value gameentities.NetValue) uint64 {
	result := uint64(0)
	result = uint64(value.GetParentObjectId()) << 16
	result = result | uint64(value.GetValueId())

	return result
}

func (n *NetValuesModel) rebuildWorldCache() {
	n.worldCache = n.worldCache[:0]

	for _, val := range n.worldManagedContainer {
		if val.GetIsDeleting() {
			continue
		}

		n.worldCache = append(n.worldCache, val)
	}
}

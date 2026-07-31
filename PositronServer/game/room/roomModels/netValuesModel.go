package roommodels

import (
	"log"
	gameentities "positron/game/gameEntities"
	"positron/internal/arena"
	"sync"
)

type NetValuesModel struct {
	mutex *sync.Mutex

	worldManagedContainer map[uint64]gameentities.NetValue
	worldPersistentArena  *arena.PersistentArena

	worldCache               []gameentities.NetValue
	worldCacheTransientArena *arena.TransientArena

	modificationCache          []gameentities.NetValue
	modificationTransientArena *arena.TransientArena

	incomingTransientArena *arena.TransientArena
}

func NewNetValuesModel() *NetValuesModel {
	return &NetValuesModel{
		mutex:                      &sync.Mutex{},
		worldManagedContainer:      make(map[uint64]gameentities.NetValue),
		worldPersistentArena:       arena.NewPersistentArena(),
		worldCache:                 make([]gameentities.NetValue, 0, 16),
		worldCacheTransientArena:   arena.NewTransfientArena(),
		modificationCache:          make([]gameentities.NetValue, 0, 16),
		modificationTransientArena: arena.NewTransfientArena(),
		incomingTransientArena:     arena.NewTransfientArena(),
	}
}

func (n *NetValuesModel) GetValues() ([]gameentities.NetValue, []byte) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	n.rebuildWorldCache()
	return n.worldCache, n.worldCacheTransientArena.ReadAll()
}

func (n *NetValuesModel) GetTempMod() ([]gameentities.NetValue, []byte) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	return n.modificationCache, n.modificationTransientArena.ReadAll()
}

func (n *NetValuesModel) ResetTempBuffers() {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	n.modificationCache = n.modificationCache[:0]
	n.modificationTransientArena.Flush()
}

func (n *NetValuesModel) PutTransientDataIncoming(data []byte) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	n.incomingTransientArena.Flush()
	n.incomingTransientArena.CloneFrom(data)
}

func (n *NetValuesModel) AddOrModify(incoming gameentities.NetValue) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	local, isExist := n.worldManagedContainer[n.getKeyOfValue(incoming)]

	if isExist && local.GetIsDeleting() {
		return
	}

	if isExist {
		n.modifyValue(incoming, local)
	} else {
		n.addValue(incoming)
	}
}

func (n *NetValuesModel) RemoveAllValuesFromObject(objectUuid uint32) {
	n.mutex.Lock()
	defer n.mutex.Unlock()

	for key, val := range n.worldManagedContainer {
		if val.GetParentObjectId() == objectUuid {
			delete(n.worldManagedContainer, key)

			if err := n.worldPersistentArena.Free(val.GetPersistentMemoryDescriptor(), false); err != nil {
				log.Printf("Unable to remove value %v %v due to memory err %v", val.GetParentObjectId(), val.GetValueId(), err)
				continue
			}

			val.MarkAsDeleting()
			val.SetPersistentMemoryDescriptor(0)
			val.SetTransientMemoryDescriptors(0, 0)

			n.modificationCache = append(n.modificationCache, val)
		}
	}
}

func (n *NetValuesModel) addValue(incoming gameentities.NetValue) {
	incomingPayload, err := n.incomingTransientArena.Read(incoming.GetTransientMemoryDescriptor())

	if err != nil {
		log.Printf("add value failed due to memory (Reading incoming) err %v", err)
	}

	descriptor := n.worldPersistentArena.Alloc(incomingPayload)
	local := gameentities.NewNetValueAsPersistent(descriptor, incoming.GetParentObjectId(), incoming.GetValueId(), incoming.GetIsDeleting())

	deltaPtr := n.modificationTransientArena.Alloc(incomingPayload)
	deltaLen := uint32(len(incomingPayload))
	incoming.SetTransientMemoryDescriptors(deltaPtr, deltaLen)

	n.worldManagedContainer[n.getKeyOfValue(local)] = local
	n.modificationCache = append(n.modificationCache, incoming)
}

func (n *NetValuesModel) modifyValue(incoming gameentities.NetValue, local gameentities.NetValue) {
	incomingPayload, err := n.incomingTransientArena.Read(incoming.GetTransientMemoryDescriptor())

	if err != nil {
		log.Printf("modify value failed due to memory (Reading incoming) err %v", err)
	}

	descriptor, err := n.worldPersistentArena.Patch(local.GetPersistentMemoryDescriptor(), incomingPayload)
	deltaPtr := n.modificationTransientArena.Alloc(incomingPayload)
	deltaLen := uint32(len(incomingPayload))

	if err != nil {
		log.Printf("modify value failed due to memory (Patch) err %v", err)
	}

	local.SetPersistentMemoryDescriptor(descriptor)
	incoming.SetTransientMemoryDescriptors(deltaPtr, deltaLen)

	n.modificationCache = append(n.modificationCache, incoming)
	n.worldManagedContainer[n.getKeyOfValue(local)] = local
}

func (n *NetValuesModel) getKeyOfValue(value gameentities.NetValue) uint64 {
	result := uint64(0)
	result = uint64(value.GetParentObjectId()) << 16
	result = result | uint64(value.GetValueId())

	return result
}

func (n *NetValuesModel) rebuildWorldCache() {
	n.worldCache = n.worldCache[:0]
	n.worldCacheTransientArena.Flush()

	for _, val := range n.worldManagedContainer {
		if val.GetIsDeleting() {
			continue
		}

		readed, err := n.worldPersistentArena.Read(val.GetPersistentMemoryDescriptor())

		if err != nil {
			log.Printf("Err while reading persistent memory %v", err)
			continue
		}

		ptr := n.worldCacheTransientArena.Alloc(readed)
		len := uint32(len(readed))

		val.SetTransientMemoryDescriptors(ptr, len)

		n.worldCache = append(n.worldCache, val)
	}
}

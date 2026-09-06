package roommodels

import (
	"log"
	gameentities "positron/game/gameEntities"
	"positron/internal/arena"
)

type NetValuesModel struct {
	gameObjectsModel ReadOnlyGameObjectsModel

	worldFlatContainer    []gameentities.NetValue
	worldFlatFreeIdsStack []uint32
	lastId                uint32
	worldPersistentArena  *arena.PersistentArena

	worldCache               []gameentities.NetValue
	worldCacheTransientArena *arena.TransientArena

	additionModidificationCache []gameentities.NetValue
	modificationCache           []gameentities.PersistentNetValue
	modificationTransientArena  *arena.TransientArena

	incomingTransientArena *arena.TransientArena
}

func NewNetValuesModel(gameObjectsModel ReadOnlyGameObjectsModel) *NetValuesModel {
	return &NetValuesModel{
		gameObjectsModel:            gameObjectsModel,
		worldFlatContainer:          make([]gameentities.NetValue, 0, 256),
		worldFlatFreeIdsStack:       make([]uint32, 0, 256),
		lastId:                      0,
		worldPersistentArena:        arena.NewPersistentArena(),
		worldCache:                  make([]gameentities.NetValue, 0, 32),
		worldCacheTransientArena:    arena.NewTransfientArena(),
		additionModidificationCache: make([]gameentities.NetValue, 0, 32),
		modificationCache:           make([]gameentities.PersistentNetValue, 0, 32),
		modificationTransientArena:  arena.NewTransfientArena(),
		incomingTransientArena:      arena.NewTransfientArena(),
	}
}

func (n *NetValuesModel) GetValues() ([]gameentities.NetValue, []byte) {
	n.rebuildWorldCache()
	return n.worldCache, n.worldCacheTransientArena.ReadAll()
}

func (n *NetValuesModel) GetAdden() []gameentities.NetValue {
	return n.additionModidificationCache
}

func (n *NetValuesModel) GetModified() []gameentities.PersistentNetValue {
	return n.modificationCache
}

func (n *NetValuesModel) ReadLocalTransientArena() []byte {
	return n.modificationTransientArena.ReadAll()
}

func (n *NetValuesModel) ResetTempBuffers() {
	n.additionModidificationCache = n.additionModidificationCache[:0]
	n.modificationCache = n.modificationCache[:0]
	n.modificationTransientArena.Flush()
}

func (n *NetValuesModel) PutTransientDataIncoming(data []byte) {
	n.incomingTransientArena.Flush()
	n.incomingTransientArena.CloneFrom(data)
}

func (n *NetValuesModel) AddValue(incoming gameentities.NetValue, attemptorId uint32, actualHost uint32) {
	if incoming.GetIsDeleting() {
		return
	}

	if owner, isExisted := n.gameObjectsModel.ThreadUnsafeGetObjectOwner(incoming.GetParentObjectId()); (owner != attemptorId && attemptorId != actualHost) || !isExisted {
		return
	}

	if _, len := incoming.GetTransientMemoryDescriptor(); len == 0 {
		return
	}

	incomingPayload, err := n.incomingTransientArena.Read(incoming.GetTransientMemoryDescriptor())

	if err != nil {
		log.Printf("add value failed due to memory (Reading incoming) err %v", err)
		return
	}

	descriptor := n.worldPersistentArena.Alloc(incomingPayload)
	localFlatContainerId := n.getFreeDescriptor()
	n.allocateChunkIfNeed(localFlatContainerId)

	local := gameentities.NewNetValueAsPersistent(descriptor, incoming.GetParentObjectId(), incoming.GetValueId(), incoming.GetIsDeleting(), localFlatContainerId)

	deltaPtr := n.modificationTransientArena.Alloc(incomingPayload)
	deltaLen := uint32(len(incomingPayload))

	incoming.SetTransientMemoryDescriptors(deltaPtr, deltaLen)
	incoming.SetArrayDescriptor(localFlatContainerId)

	n.worldFlatContainer[localFlatContainerId] = local
	n.additionModidificationCache = append(n.additionModidificationCache, incoming)
}

func (n *NetValuesModel) ModifyValue(incoming gameentities.PersistentNetValue, attemptorClientId uint32, actualHost uint32) {
	if int64(incoming.GetArrayDescriptor()) >= int64(len(n.worldFlatContainer)) {
		return
	}

	local := n.worldFlatContainer[incoming.GetArrayDescriptor()]

	if local.GetParentObjectId() == 0 {
		return
	}

	owner, isExists := n.gameObjectsModel.ThreadUnsafeGetObjectOwner(local.GetParentObjectId())

	if !isExists {
		return
	}

	if _, len := incoming.GetTransientMemoryDescriptor(); len == 0 {
		return
	}

	if owner != attemptorClientId && attemptorClientId != actualHost {
		return
	}

	log.Println(owner, attemptorClientId, actualHost)

	n.modifyValue(incoming, local)
}

func (n *NetValuesModel) RemoveAllValuesFromObject(objectId uint32) {
	for i := range n.worldFlatContainer {
		val := n.worldFlatContainer[i]
		valDesc := val.GetArrayDescriptor()

		if val.GetParentObjectId() == 0 || val.GetParentObjectId() != objectId {
			continue
		}

		if err := n.worldPersistentArena.Free(val.GetPersistentMemoryDescriptor(), false); err != nil {
			log.Printf("Unable to remove value %v %v due to memory err %v", val.GetParentObjectId(), val.GetValueId(), err)
			continue
		}

		val.ResetParentObjectId()

		n.worldFlatContainer[valDesc] = val
		n.worldFlatFreeIdsStack = append(n.worldFlatFreeIdsStack, valDesc)

		val.MarkAsDeleting()
		val.SetPersistentMemoryDescriptor(0)
		val.SetTransientMemoryDescriptors(0, 0)

		n.worldFlatContainer[valDesc] = val

		n.modificationCache = append(n.modificationCache, gameentities.NewPersistentNetValue(valDesc, 0, 0, true))
	}
}

func (n *NetValuesModel) modifyValue(incoming gameentities.PersistentNetValue, local gameentities.NetValue) {
	incomingPayload, err := n.incomingTransientArena.Read(incoming.GetTransientMemoryDescriptor())

	if err != nil {
		log.Printf("modify value failed due to memory (Reading incoming) err %v", err)
		return
	}

	descriptor, err := n.worldPersistentArena.Patch(local.GetPersistentMemoryDescriptor(), incomingPayload)
	deltaPtr := n.modificationTransientArena.Alloc(incomingPayload)
	deltaLen := uint32(len(incomingPayload))

	if err != nil {
		_, l := incoming.GetTransientMemoryDescriptor()
		log.Printf("modify value failed due to memory (Patch) err %v. obj id %v, desc %v, len %v", err, local.GetParentObjectId(), local.GetPersistentMemoryDescriptor(), l)
		return
	}

	local.SetPersistentMemoryDescriptor(descriptor)
	incoming.SetTransientMemoryDescriptors(deltaPtr, deltaLen)

	n.modificationCache = append(n.modificationCache, incoming)
	n.worldFlatContainer[local.GetArrayDescriptor()] = local
}

func (n *NetValuesModel) getFreeDescriptor() uint32 {
	if len(n.worldFlatFreeIdsStack) > 0 {
		id := n.worldFlatFreeIdsStack[len(n.worldFlatFreeIdsStack)-1]
		n.worldFlatFreeIdsStack = n.worldFlatFreeIdsStack[:len(n.worldFlatFreeIdsStack)-1]

		return id
	}

	n.lastId++
	return n.lastId
}

func (g *NetValuesModel) allocateChunkIfNeed(id uint32) {
	if int64(id) >= int64(len(g.worldFlatContainer)) {
		g.worldFlatContainer = append(g.worldFlatContainer, make([]gameentities.NetValue, ALLOCATION_CHUNK)...)
	}
}

func (n *NetValuesModel) rebuildWorldCache() {
	n.worldCache = n.worldCache[:0]
	n.worldCacheTransientArena.Flush()

	for i := range n.worldFlatContainer {
		val := n.worldFlatContainer[i]

		if val.GetParentObjectId() == 0 {
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

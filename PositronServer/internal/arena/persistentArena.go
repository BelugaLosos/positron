package arena

import (
	"errors"
	"math"
	"math/bits"
	diagnosticsdata "positron/internal/diagnosticsData"
)

type PersistentArena struct {
	meta               []Block
	freeSlotContainers []FreeMemorySearchContainer
	freeBlocksCount    int

	buffer []byte

	writePtr uint32

	metrics *diagnosticsdata.ArenaMetrics
}

const (
	ALLOC_CHUNK  = 512
	MIN_TO_SPLIT = 256
)

var (
	ErrInvalidDescriptor   = errors.New("Invalid descriptor out of range")
	ErrFreeMemoryViolation = errors.New("Can`t free already free memory")
)

func NewPersistentArena() *PersistentArena {
	allocated := &PersistentArena{
		meta:               make([]Block, 0, 32),
		freeSlotContainers: make([]FreeMemorySearchContainer, 0),
		freeBlocksCount:    0,
		buffer:             make([]byte, ALLOC_CHUNK),
		writePtr:           0,
		metrics: &diagnosticsdata.ArenaMetrics{
			DescriptorsCount:   0,
			AllocatedSize:      0,
			UsedSize:           0,
			AllocWithReuse:     0,
			AllocWithMalloc:    0,
			PatchWithReuse:     0,
			PatchWithRealloc:   0,
			FragmentationRatio: 0.0,
			FreeDescriptors:    make([]diagnosticsdata.ArenaFreeDescriotorsCalssMetrics, 0),
		},
	}

	for i := range 15 {
		power := i + 1

		slotsContainer := FreeMemorySearchContainer{
			size:  uint32(math.Pow(2, float64(power))),
			slots: make([]int, 0, 32),
		}

		slotMetrics := diagnosticsdata.ArenaFreeDescriotorsCalssMetrics{
			Size:                 int(slotsContainer.size),
			FreeDescriptorsCount: 0,
		}

		allocated.freeSlotContainers = append(allocated.freeSlotContainers, slotsContainer)
		allocated.metrics.FreeDescriptors = append(allocated.metrics.FreeDescriptors, slotMetrics)
	}

	return allocated
}

func (p *PersistentArena) Alloc(data []byte) int {
	dataLen := uint32(len(data))

	hasFree, descriptor := p.tryFindFreeSlot(dataLen)

	var dst Block

	if hasFree {
		meta := p.meta[descriptor]
		oldCap := meta.cap

		remaining := oldCap - dataLen

		if remaining >= MIN_TO_SPLIT {
			meta.len = dataLen
			meta.cap = dataLen
			meta.used = true
			p.meta[descriptor] = meta

			splitted := Block{
				ptr:  meta.ptr + dataLen,
				len:  0,
				cap:  remaining,
				used: false,
			}

			contIdx := p.findContainerFor(splitted.cap)
			p.meta = append(p.meta, splitted)
			p.freeSlotContainers[contIdx].slots = append(p.freeSlotContainers[contIdx].slots, len(p.meta)-1)
		} else {
			meta.len = dataLen
			meta.used = true
			p.meta[descriptor] = meta

			p.freeBlocksCount--
		}

		dst = p.meta[descriptor]

		p.metrics.AllocWithReuse++
	} else {
		p.checkAndResize(dataLen)

		dst = Block{
			ptr:  p.writePtr,
			len:  dataLen,
			cap:  dataLen,
			used: true,
		}

		descriptor = len(p.meta)
		p.meta = append(p.meta, dst)

		p.writePtr += dataLen

		p.metrics.AllocWithMalloc++
	}

	copy(p.buffer[dst.ptr:dst.ptr+dataLen], data)

	return descriptor
}

func (p *PersistentArena) Free(descriptor int, doFullSegmentClear bool) error {
	if descriptor < 0 || descriptor >= len(p.meta) {
		return ErrInvalidDescriptor
	}

	block := p.meta[descriptor]

	if !block.used {
		return ErrFreeMemoryViolation
	}

	if doFullSegmentClear {
		clear(p.buffer[block.ptr:(block.ptr + block.cap)])
	}

	block.len = 0
	block.used = false

	p.meta[descriptor] = block

	contIndex := p.findContainerFor(block.cap)
	p.freeSlotContainers[contIndex].slots = append(p.freeSlotContainers[contIndex].slots, descriptor)
	p.freeBlocksCount++

	return nil
}

func (p *PersistentArena) Patch(descriptor int, newData []byte) (int, error) {
	if descriptor < 0 || descriptor >= len(p.meta) {
		return 0, ErrInvalidDescriptor
	}

	block := p.meta[descriptor]

	if !block.used {
		return 0, ErrFreeMemoryViolation
	}

	dataLen := uint32(len(newData))

	if block.cap >= dataLen {
		copy(p.buffer[block.ptr:(block.ptr+dataLen)], newData)

		if block.len != dataLen {
			block.len = dataLen
			p.meta[descriptor] = block
		}

		p.metrics.PatchWithReuse++

		return descriptor, nil
	}

	p.metrics.PatchWithRealloc++

	if err := p.Free(descriptor, false); err != nil {
		return 0, err
	}

	return p.Alloc(newData), nil
}

func (p *PersistentArena) Read(descriptor int) ([]byte, error) {
	if descriptor < 0 || descriptor >= len(p.meta) {
		return nil, ErrInvalidDescriptor
	}

	block := p.meta[descriptor]

	if !block.used {
		return nil, ErrFreeMemoryViolation
	}

	return p.buffer[block.ptr:(block.ptr + block.len)], nil
}

func (p *PersistentArena) CollectMetrics() *diagnosticsdata.ArenaMetrics {
	used, ratio := p.estimateFragmentationPercent()

	p.metrics.DescriptorsCount = len(p.meta)
	p.metrics.AllocatedSize = len(p.buffer)
	p.metrics.UsedSize = used
	p.metrics.FragmentationRatio = ratio

	for i := range p.metrics.FreeDescriptors {
		p.metrics.FreeDescriptors[i].FreeDescriptorsCount = len(p.freeSlotContainers[i].slots)
	}

	return p.metrics
}

func (p *PersistentArena) estimateFragmentationPercent() (int, float64) {
	used := uint32(0)

	for desc := range p.meta {
		if p.meta[desc].used {
			used += p.meta[desc].len
		}
	}

	return int(used), float64(p.writePtr-used) / float64(p.writePtr)
}

func (p *PersistentArena) checkAndResize(dataLen uint32) {
	buffLen := uint32(len(p.buffer))

	if (dataLen + p.writePtr) <= buffLen {
		return
	}

	need := p.writePtr + dataLen
	step := 1

	for uint32(len(p.buffer)) < need {
		p.buffer = append(p.buffer, make([]byte, ALLOC_CHUNK*step)...)
		step++

		if step > 3 {
			step = 3
		}
	}
}

func (p *PersistentArena) tryFindFreeSlot(dataLen uint32) (bool, int) {
	if p.freeBlocksCount == 0 {
		return false, 0
	}

	i := p.findContainerFor(dataLen)

	for i < len(p.freeSlotContainers) {
		finded, slot := p.findSlotInCont(i, dataLen)

		if finded {
			return true, slot
		}

		i++
	}

	return false, 0
}

func (p *PersistentArena) findContainerFor(dataLen uint32) int {
	i := bits.Len32(dataLen-1) - 1

	if i >= len(p.freeSlotContainers) {
		i = len(p.freeSlotContainers) - 1
	}

	if i < 0 {
		i = 0
	}

	return i
}

func (p *PersistentArena) findSlotInCont(i int, dataLen uint32) (bool, int) {
	cont := p.freeSlotContainers[i]

	for j := range cont.slots {
		slotIndex := cont.slots[j]

		if slotIndex == -1 {
			continue
		}

		slot := p.meta[slotIndex]

		if slot.cap >= dataLen {
			cont.slots[j] = cont.slots[len(cont.slots)-1]
			cont.slots = cont.slots[:(len(cont.slots) - 1)]

			p.freeSlotContainers[i] = cont
			return true, slotIndex
		}
	}

	p.freeSlotContainers[i] = cont

	return false, 0
}

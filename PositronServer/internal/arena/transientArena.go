package arena

type TransientArena struct {
	buff     []byte
	writePtr uint32
}

func NewTransfientArena() *TransientArena {
	return &TransientArena{
		buff:     make([]byte, ALLOC_CHUNK),
		writePtr: 0,
	}
}

func (t *TransientArena) CloneFrom(externalBuffer []byte) {
	t.checkAndResizeAbsolute(uint32(len(externalBuffer)))
	t.writePtr = uint32(len(externalBuffer))
	copy(t.buff, externalBuffer)
}

func (t *TransientArena) Alloc(data []byte) uint32 {
	dataLen := uint32(len(data))

	t.checkAndResize(dataLen)

	copy(t.buff[t.writePtr:t.writePtr+dataLen], data)
	ptr := t.writePtr
	t.writePtr += dataLen

	return ptr
}

func (t *TransientArena) Read(ptr, len uint32) ([]byte, error) {
	if (ptr + len) > t.writePtr {
		return nil, ErrInvalidDescriptor
	}

	return t.buff[ptr:(ptr + len)], nil
}

func (t *TransientArena) ReadAll() []byte {
	return t.buff[:t.writePtr]
}

func (t *TransientArena) Flush() {
	clear(t.buff)
	t.writePtr = 0
}

func (t *TransientArena) GetActualSize() int {
	return int(t.writePtr)
}

func (t *TransientArena) checkAndResize(dataLen uint32) {
	buffLen := uint32(len(t.buff))

	if (dataLen + t.writePtr) <= buffLen {
		return
	}

	need := t.writePtr + dataLen
	t.checkAndResizeAbsolute(need)
}

func (t *TransientArena) checkAndResizeAbsolute(need uint32) {
	if uint32(len(t.buff)) >= need {
		return
	}

	for uint32(len(t.buff)) < need {
		t.buff = append(t.buff, make([]byte, ALLOC_CHUNK)...)
	}
}

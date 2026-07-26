package arena

type Block struct {
	ptr  uint32
	len  uint32
	cap  uint32
	used bool
}

type FreeMemorySearchContainer struct {
	size  uint32
	slots []int
}

func NewBlock(ptr, len uint32) Block {
	return Block{
		ptr: ptr,
		len: len,
	}
}

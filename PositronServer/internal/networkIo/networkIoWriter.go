package networkio

import "encoding/binary"

type NetworkIoWriter struct {
	buff []byte
	ptr  uint32
}

func NewNetworkWriter() *NetworkIoWriter {
	return &NetworkIoWriter{}
}

func (n *NetworkIoWriter) Wrap(from []byte) {
	n.buff = from
	n.ptr = 0
}

func (n *NetworkIoWriter) Free() {
	n.buff = nil
	n.ptr = 0
}

func (n *NetworkIoWriter) WriteUint32(value uint32) {
	binary.BigEndian.PutUint32(n.pick(4), value)
}

func (n *NetworkIoWriter) WriteSegment(segment []byte) {
	copy(n.buff[n.ptr:], segment)
	n.ptr += uint32(len(segment))
}

func (n *NetworkIoWriter) pick(len uint32) []byte {
	seg := n.buff[n.ptr : n.ptr+len]
	n.ptr += len

	return seg
}

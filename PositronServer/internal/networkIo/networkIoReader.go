package networkio

import "encoding/binary"

type NetworkIoReader struct {
	buff []byte
	ptr  uint32
}

func NewNetworkReader() *NetworkIoReader {
	return &NetworkIoReader{}
}

func (n *NetworkIoReader) Wrap(from []byte) {
	n.buff = from
	n.ptr = 0
}

func (n *NetworkIoReader) Free() {
	n.buff = nil
	n.ptr = 0
}

func (n *NetworkIoReader) ReadUint32() uint32 {
	return binary.BigEndian.Uint32(n.ReadSegment(4))
}

func (n *NetworkIoReader) ReadSegment(len uint32) []byte {
	seg := n.buff[n.ptr : n.ptr+len]
	n.ptr += len

	return seg
}

package tests

import (
	networkio "positron/internal/networkIo"
	"testing"
)

func TestRw(t *testing.T) {
	buffer := make([]byte, 1024)
	str := generateRandomString(512)

	writter := networkio.NewNetworkWriter()
	writter.Wrap(buffer)

	writter.WriteUint32(6)
	writter.WriteUint32(7)
	writter.WriteSegment([]byte(str))
	writter.WriteUint32(5)

	writter.Free()

	reader := networkio.NewNetworkReader()
	reader.Wrap(buffer)

	if n := reader.ReadUint32(); n != 6 {
		t.Error("Read uint32 err")
	}

	if n := reader.ReadUint32(); n != 7 {
		t.Error("Read uint32 err")
	}

	if s := reader.ReadSegment(512); string(s) != str {
		t.Error("Read string err")
	}

	if n := reader.ReadUint32(); n != 5 {
		t.Error("Read uint32 err")
	}

	reader.Free()
}

package util

import (
	"encoding/binary"
)

func GlueDataToOptions(eventType byte, isCompressed bool, sourceLength uint32, rawData []byte) []byte {
	compressionFlag := byte(0)
	totalLen := len(rawData) + 2

	if isCompressed {
		compressionFlag = 1
		totalLen += 4
	}

	buf := make([]byte, totalLen)
	buf[0] = eventType
	buf[1] = byte(compressionFlag)

	if isCompressed {
		binary.BigEndian.PutUint32(buf[2:6], sourceLength)
		copy(buf[6:], rawData)
	} else {
		copy(buf[2:], rawData)
	}

	return buf
}

func DeconstructPacket(packet []byte) (byte, bool, uint32, []byte) {
	eventType := packet[0]
	isCompressed := packet[1] == 1
	sourceDataLen := uint32(0)
	var data []byte

	if isCompressed {
		data = packet[6:]
		sourceDataLen = uint32(binary.BigEndian.Uint32(packet[2:6]))
	} else {
		data = packet[2:]
		sourceDataLen = uint32(len(data))
	}

	return eventType, isCompressed, uint32(sourceDataLen), data
}

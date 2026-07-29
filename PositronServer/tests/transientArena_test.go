package tests

import (
	"positron/internal/arena"
	"testing"
)

func TestCloneAndReadAll(t *testing.T) {
	str := generateRandomString(1500)
	aren := arena.NewTransfientArena()
	aren.CloneFrom([]byte(str))

	if b := aren.ReadAll(); string(b) != str {
		t.Errorf("data corrupted with read all %v != %v", string(b), str)
	}

	if aren.GetActualSize() != 1500 {
		t.Errorf("size corrupted %v", aren.GetActualSize())
	}
}

func TestAllocAndReadTransient(t *testing.T) {
	aren := arena.NewTransfientArena()

	for range 10 {
		str := generateRandomString(200)
		ptr := aren.Alloc([]byte(str))

		if b, err := aren.Read(ptr, uint32(len(str))); string(b) != str || err != nil {
			t.Errorf("corrupt or err %v; %e", string(b) == str, err)
		}
	}

	if aren.GetActualSize() != 2000 {
		t.Errorf("size corrupted %v", aren.GetActualSize())
	}
}

func TestFlushTransient(t *testing.T) {
	str := generateRandomString(1500)
	aren := arena.NewTransfientArena()
	aren.CloneFrom([]byte(str))

	if b := aren.ReadAll(); string(b) != str {
		t.Errorf("data corrupted with read all %v != %v", string(b), str)
	}

	aren.Flush()

	if aren.GetActualSize() != 0 {
		t.Error("non efficient flush")
	}

	str = generateRandomString(250)

	aren.CloneFrom([]byte(str))

	if b := aren.ReadAll(); string(b) != str {
		t.Errorf("data corrupted with read all %v != %v", string(b), str)
	}
}

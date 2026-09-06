package tests

import (
	"errors"
	"log"
	"positron/internal/arena"
	"testing"
)

func TestAlloc(t *testing.T) {
	arena := arena.NewPersistentArena()

	str := generateRandomString(513)
	descriptor := arena.Alloc([]byte(str))

	if descriptor != 0 {
		t.Error("invalid generated descriptor")
	}

	if b, err := arena.Read(descriptor); err != nil || string(b) != str {
		t.Errorf("string not equals: %v \t err: %e", string(b) != str, err)
	}

	metrics := arena.CollectMetrics()

	if metrics.DescriptorsCount != 1 {
		t.Errorf("Invalid descriptors count %v", metrics.DescriptorsCount)
	}

	if metrics.AllocatedSize != 1024 {
		t.Errorf("Invalid memory count %v", metrics.AllocatedSize)
	}

	if metrics.UsedSize != 513 {
		t.Errorf("Invalid used memory count %v", metrics.UsedSize)
	}

	if metrics.AllocWithReuse != 0 {
		t.Errorf("First allocation and with reuse ? %v", metrics.AllocWithReuse)
	}

	if metrics.AllocWithMalloc != 1 {
		t.Errorf("Invalid mallocs count %v", metrics.AllocWithReuse)
	}

	if metrics.PatchWithRealloc != 0 || metrics.PatchWithReuse != 0 {
		t.Error("There no patches yet but counts are incremented!?")
	}

	if metrics.FragmentationRatio != 0 {
		t.Errorf("Invalid fragmentation ratio %v", metrics.FragmentationRatio)
	}
}

func TestFree(t *testing.T) {
	aren := arena.NewPersistentArena()

	str := generateRandomString(513)
	descriptor := aren.Alloc([]byte(str))

	if descriptor != 0 {
		t.Error("invalid generated descriptor")
	}

	if b, err := aren.Read(descriptor); err != nil || string(b) != str {
		t.Errorf("string not equals: %v \t err: %e", string(b) != str, err)
	}

	if err := aren.Free(descriptor, false); err != nil {
		t.Error(err)
	}

	if _, err := aren.Read(descriptor); !errors.Is(err, arena.ErrFreeMemoryViolation) {
		t.Error(err)
	}

	metrics := aren.CollectMetrics()

	if metrics.DescriptorsCount != 1 {
		t.Errorf("Invalid descriptors count %v", metrics.DescriptorsCount)
	}

	if metrics.AllocatedSize != 1024 {
		t.Errorf("Invalid memory count %v", metrics.AllocatedSize)
	}

	if metrics.UsedSize != 0 {
		t.Errorf("Invalid used memory count %v", metrics.UsedSize)
	}

	if metrics.AllocWithReuse != 0 {
		t.Errorf("First allocation and with reuse ? %v", metrics.AllocWithReuse)
	}

	if metrics.AllocWithMalloc != 1 {
		t.Errorf("Invalid mallocs count %v", metrics.AllocWithReuse)
	}

	if metrics.PatchWithRealloc != 0 || metrics.PatchWithReuse != 0 {
		t.Error("There no patches yet but counts are incremented!?")
	}

	if metrics.FragmentationRatio != 1 {
		t.Errorf("Invalid fragmentation ratio %v", metrics.FragmentationRatio)
	}
}

func TestPatch(t *testing.T) {
	aren := arena.NewPersistentArena()

	str := generateRandomString(513)
	descriptor := aren.Alloc([]byte(str))

	if descriptor != 0 {
		t.Error("invalid generated descriptor")
	}

	patchStr := generateRandomString(500)
	if patchDescriptor, err := aren.Patch(descriptor, []byte(patchStr)); patchDescriptor != descriptor || err != nil {
		t.Errorf("Descriptors validity: %v \t err: %e", patchDescriptor == descriptor, err)
	}

	anotherPatch := generateRandomString(600)
	if anotherPathDescriptor, err := aren.Patch(descriptor, []byte(anotherPatch)); anotherPathDescriptor == descriptor || err != nil {
		t.Errorf("Descriptors validity: %v \t err: %e", anotherPathDescriptor != descriptor, err)
	}

	metrics := aren.CollectMetrics()

	if metrics.DescriptorsCount != 2 {
		t.Errorf("Invalid descriptors count %v", metrics.DescriptorsCount)
	}

	if metrics.AllocatedSize != 1536 {
		t.Errorf("Invalid memory count %v", metrics.AllocatedSize)
	}

	if metrics.UsedSize != 600 {
		t.Errorf("Invalid used memory count %v", metrics.UsedSize)
	}

	if metrics.AllocWithReuse != 0 {
		t.Errorf("First allocation and with reuse ? %v", metrics.AllocWithReuse)
	}

	if metrics.AllocWithMalloc != 2 {
		t.Errorf("Invalid mallocs count %v", metrics.AllocWithReuse)
	}

	if metrics.PatchWithRealloc != 1 || metrics.PatchWithReuse != 1 {
		t.Errorf("Invalid patches %v %v", metrics.PatchWithRealloc, metrics.PatchWithReuse)
	}

	log.Println(metrics.FragmentationRatio)
}

func TestReplicativeDescriptorsInvalidation(t *testing.T) {
	aren := arena.NewPersistentArena()

	str := generateRandomString(16)

	d1 := aren.Alloc([]byte(str)) //0
	d2 := aren.Alloc([]byte(str)) //1
	d3 := aren.Alloc([]byte(str)) //2
	d4 := aren.Alloc([]byte(str)) //3

	if err := aren.Free(d1, true); err != nil {
		t.Error(err) //3
	}

	if err := aren.Free(d2, true); err != nil {
		t.Error(err) //2
	}

	if err := aren.Free(d3, true); err != nil {
		t.Error(err) //1
	}

	if err := aren.Free(d4, true); err != nil {
		t.Error(err) //0
	}

	r1 := aren.Alloc([]byte(str))
	r2 := aren.Alloc([]byte(str))
	r3 := aren.Alloc([]byte(str))
	r4 := aren.Alloc([]byte(str))

	if r1 != 0 || r2 != 3 || r3 != 2 || r4 != 1 {
		t.Errorf("invalid %v %v %v %v", r1, r2, r3, r4)
	}
}

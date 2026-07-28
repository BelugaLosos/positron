package diagnosticsdata

type ArenaMetrics struct {
	DescriptorsCount int
	AllocatedSize    int
	UsedSize         int

	AllocWithReuse  int
	AllocWithMalloc int

	PatchWithReuse   int
	PatchWithRealloc int

	FragmentationRatio float64

	FreeDescriptors []ArenaFreeDescriotorsCalssMetrics
}

type ArenaFreeDescriotorsCalssMetrics struct {
	Size                 int
	FreeDescriptorsCount int
}

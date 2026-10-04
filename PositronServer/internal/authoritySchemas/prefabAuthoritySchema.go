package authorityschemas

type PrefabAuthoritySchema struct {
	SpawnAuthoruty             byte `json:"_spawnAuthority"`
	DestroyAuthority           byte `json:"_destroyAuthority"`
	OwnershipTransferAuthority byte `json:"_ownershipAuthority"`
}

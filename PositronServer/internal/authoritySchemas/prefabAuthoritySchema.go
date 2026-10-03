package authorityschemas

type PrefabAuthoritySchema struct {
	Id                         uint16 `json:"_id"`
	SpawnAuthoruty             byte   `json:"_spawnAuthority"`
	DestroyAuthority           byte   `json:"_destroyAuthority"`
	OwnershipTransferAuthority byte   `json:"_ownershipAuthority"`
}

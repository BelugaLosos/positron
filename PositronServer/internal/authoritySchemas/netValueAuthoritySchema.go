package authorityschemas

type NetValueAuthoritySchema struct {
	PrefabId                 uint16 `json:"_prefabId"`
	ValueId                  uint16 `json:"_valueIdOnObject"`
	NetValueChangesAuthority byte   `json:"_auhority"`
}

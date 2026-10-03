package authorityschemas

type AuthoritySchemaDto struct {
	PrefabSchemas    []PrefabAuthoritySchema   `json:"_prefabsAuthoritySchemas"`
	NetValuesSchemas []NetValueAuthoritySchema `json:"_netValuesAuthoritySchemas"`
}

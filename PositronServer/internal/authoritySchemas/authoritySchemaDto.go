package authorityschemas

type AuthoritySchemaDto struct {
	PrefabSchemas       []PrefabAuthoritySchema                  `json:"_prefabsAuthoritySchemas"`
	ObjectWithNetValues []NetValueAuthorityObjectContainerSchema `json:"_netValuesAuthoritySchemas"`
}

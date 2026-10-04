package authorityschemas

type NetValueAuthorityObjectContainerSchema struct {
	ContainedNetValueSchemas []NetValueAuthoritySchema `json:"_netValuesOfObject"`
}

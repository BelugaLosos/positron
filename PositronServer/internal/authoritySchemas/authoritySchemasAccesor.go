package authorityschemas

import (
	"encoding/json"
	"errors"
	"fmt"
	"log"
	"os"
)

type AuthoritySchemasAccesor struct {
	schema  *AuthoritySchemaDto
	isReady bool
}

func NewAuthoritySchemasAccesor() *AuthoritySchemasAccesor {
	return &AuthoritySchemasAccesor{}
}

func (a *AuthoritySchemasAccesor) MustLoad() {
	data, err := os.ReadFile(fmt.Sprintf("./%s", AUTHORITY_SCHEMA_FILE_NAME))

	if err != nil {
		panic(fmt.Sprintf("Unable to start server due to configuration loading fault (authority schema loading fault). err: %s", err))
	}

	if err = a.ParseJson(data); err != nil {
		panic(err)
	}

	log.Println("Authority schemas loaded successfully!")
}

func (a *AuthoritySchemasAccesor) ParseJson(data []byte) error {
	if a.isReady {
		return errors.New("Can`t parse config twice")
	}

	a.schema = &AuthoritySchemaDto{}

	if err := json.Unmarshal(data, a.schema); err != nil {
		return fmt.Errorf("Unable to start server due to configuration parsing fault (authority schema parsing fault). err: %s", err)
	}

	a.isReady = true

	return nil
}

func (a *AuthoritySchemasAccesor) GetPrefabsCount() int {
	return len(a.schema.PrefabSchemas)
}

func (a *AuthoritySchemasAccesor) GetNetValuesCount() int {
	return len(a.schema.ObjectWithNetValues)
}

func (a *AuthoritySchemasAccesor) GetPrefabAuthority(prefabId uint16) (PrefabAuthoritySchema, error) {
	if int64(prefabId) >= int64(len(a.schema.PrefabSchemas)) {
		return PrefabAuthoritySchema{}, fmt.Errorf("Out of index schema access prevented panic")
	}

	return a.schema.PrefabSchemas[prefabId], nil
}

func (a *AuthoritySchemasAccesor) GetNetValueAuthority(prefabId, valueOnObjectId uint16) (NetValueAuthoritySchema, error) {
	if int64(prefabId) >= int64(len(a.schema.ObjectWithNetValues)) {
		return NetValueAuthoritySchema{}, fmt.Errorf("Out of index schema access prevent panic (net values object access) prefabId %v valueOnObjectId %v", prefabId, valueOnObjectId)
	}

	if int64(valueOnObjectId) >= int64(len(a.schema.ObjectWithNetValues[prefabId].ContainedNetValueSchemas)) {
		return NetValueAuthoritySchema{}, fmt.Errorf("Out of index schema access prevent panic (net values net value on object access) prefabId %v valueOnObjectId %v", prefabId, valueOnObjectId)
	}

	return a.schema.ObjectWithNetValues[prefabId].ContainedNetValueSchemas[valueOnObjectId], nil
}

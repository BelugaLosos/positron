package authorityschemas

import (
	"encoding/json"
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
		panic(fmt.Sprintf("Unable to start server due to configuration loading fault (authority schema loading fault). err: %e", err))
	}

	if err = a.ParseJson(data); err != nil {
		panic(err)
	}

	log.Println("Authority schemas loaded successfully!")
}

func (a *AuthoritySchemasAccesor) ParseJson(data []byte) error {
	a.schema = &AuthoritySchemaDto{}

	if err := json.Unmarshal(data, a.schema); err != nil {
		return fmt.Errorf("Unable to start server due to configuration parsing fault (authority schema parsing fault). err: %s", err)
	}

	return nil
}

func (a *AuthoritySchemasAccesor) GetPrefabsCount() int {
	return len(a.schema.PrefabSchemas)
}

func (a *AuthoritySchemasAccesor) GetNetValuesCount() int {
	return len(a.schema.NetValuesSchemas)
}

func (a *AuthoritySchemasAccesor) GetPrefabAuthority(prefabId uint16) PrefabAuthoritySchema {
	return a.schema.PrefabSchemas[prefabId]
}

// TODO: Remake NetValues authority and move it from flat array to hierarchyd list
// istead of value: ---
// make value obj conts (indexed like objects) (0..N)
//		|
//		|-----------> net value (0)
//		|-----------> net value (1)
//		|-----------> net value (N)
// that can improve search performance from O(N^2) to O(2) !!!

package tests

import (
	authorityschemas "positron/internal/authoritySchemas"
	"testing"
)

func TestParsingJson(t *testing.T) {
	mockJson := `{"_prefabsAuthoritySchemas":[{"_id": 3,"_spawnAuthority": 1,"_destroyAuthority": 2,"_ownershipAuthority": 2}],"_netValuesAuthoritySchemas":[{"_prefabId": 3,"_valueIdOnObject": 2,"_auhority": 1}]}`
	accesor := authorityschemas.NewAuthoritySchemasAccesor()

	if err := accesor.ParseJson([]byte(mockJson)); err != nil {
		t.Error(err)
	}

	if accesor.GetPrefabsCount() != 1 {
		t.Error("data corrupted")
	}

	if accesor.GetNetValuesCount() != 1 {
		t.Error("data corrupted")
	}
}

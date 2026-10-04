package tests

import (
	authorityschemas "positron/internal/authoritySchemas"
	"testing"
)

func TestParsingJson(t *testing.T) {
	mockJson := `{"_prefabsAuthoritySchemas":[{"_spawnAuthority": 2,"_destroyAuthority": 1,"_ownershipAuthority":2}],"_netValuesAuthoritySchemas":[{"_netValuesOfObject":[{"_auhority":1}]}]}`
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

	if scheme, err := accesor.GetPrefabAuthority(0); err != nil || scheme.SpawnAuthoruty != 2 || scheme.DestroyAuthority != 1 || scheme.OwnershipTransferAuthority != 2 {
		t.Errorf("Object authority data corrupted %v %v", err, scheme)
	}

	if scheme, err := accesor.GetNetValueAuthority(0, 0); err != nil || scheme.NetValueChangesAuthority != 1 {
		t.Errorf("Net value authority data corrupted %v %v", err, scheme)
	}
}

func TestConfigAccessViolationProof(t *testing.T) {
	mockJson := `{"_prefabsAuthoritySchemas":[{"_spawnAuthority": 2,"_destroyAuthority": 1,"_ownershipAuthority":2}],"_netValuesAuthoritySchemas":[{"_netValuesOfObject":[{"_auhority":1}]}]}`
	accesor := authorityschemas.NewAuthoritySchemasAccesor()

	if err := accesor.ParseJson([]byte(mockJson)); err != nil {
		t.Error(err)
	}

	if _, err := accesor.GetPrefabAuthority(1); err == nil {
		t.Error("Protection is not working")
	}

	if _, err := accesor.GetPrefabAuthority(0); err != nil {
		t.Error("Unexpected error wrihe reading valid object id")
	}

	if _, err := accesor.GetNetValueAuthority(1, 0); err == nil {
		t.Error("Protection is not working")
	}

	if _, err := accesor.GetNetValueAuthority(0, 1); err == nil {
		t.Error("Protection is not working")
	}

	if _, err := accesor.GetNetValueAuthority(0, 0); err != nil {
		t.Error("Unexpected error while reading valid combination of prefId and valId")
	}
}

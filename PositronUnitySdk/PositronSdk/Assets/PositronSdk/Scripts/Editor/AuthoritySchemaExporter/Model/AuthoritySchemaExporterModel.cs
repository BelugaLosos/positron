using Positron.Client.Mono;
using Positron.Client.NetValues;
using Positron.Editor.AuthoritySchemaExporter.Data;
using System.Collections.Generic;

namespace Positron.Editor.AuthoritySchemaExporter
{
    public sealed class AuthoritySchemaExporterModel
    {
        public AuthoritySchemaDto ExportObject(PositronNetworkIdentity[] prefabs)
        {
            PrefabAuthoritySchema[] prefabSchemas = ExportPrefabAuthoritySchemas(prefabs);
            NetValueAuthoritySchema[] netValuesSchemas = ExportNetValuesAuthoritySchemas(prefabs);
            AuthoritySchemaDto combinedSchema = new(prefabSchemas, netValuesSchemas);

            return combinedSchema;
        }

        private PrefabAuthoritySchema[] ExportPrefabAuthoritySchemas(PositronNetworkIdentity[] prefabs)
        {
            PrefabAuthoritySchema[] prefabSchema = new PrefabAuthoritySchema[prefabs.Length];

            for (ushort i = 0; i < prefabs.Length; i++) 
            {
                prefabSchema[i] = new(i, prefabs[i].SpawnAuthority, prefabs[i].DestroyAuthority, prefabs[i].OwnershipTransferAuthority);
            }

            return prefabSchema;
        }

        private NetValueAuthoritySchema[] ExportNetValuesAuthoritySchemas(PositronNetworkIdentity[] prefabs)
        {
            List<NetValueAuthoritySchema> schema = new();

            for (ushort i = 0; i < prefabs.Length; i++)
            {
                INetValueManaged[] netValues = prefabs[i].GetAllNetValues();

                for (ushort j = 0; j < netValues.Length; j++)
                {
                    schema.Add(new(i, j, NetValueAuthority.Owner));
                }
            }

            return schema.ToArray();
        }
    }
}
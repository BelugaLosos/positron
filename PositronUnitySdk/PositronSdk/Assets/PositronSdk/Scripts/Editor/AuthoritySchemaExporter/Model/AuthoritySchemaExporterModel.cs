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
            NetValueAuthorityObjectContainerSchema[] netValuesSchemas = ExportNetValuesAuthoritySchemas(prefabs);
            AuthoritySchemaDto combinedSchema = new(prefabSchemas, netValuesSchemas);

            return combinedSchema;
        }

        private PrefabAuthoritySchema[] ExportPrefabAuthoritySchemas(PositronNetworkIdentity[] prefabs)
        {
            PrefabAuthoritySchema[] prefabSchema = new PrefabAuthoritySchema[prefabs.Length];

            for (ushort i = 0; i < prefabs.Length; i++) 
            {
                prefabSchema[i] = new(prefabs[i].SpawnAuthority, prefabs[i].DestroyAuthority, prefabs[i].OwnershipTransferAuthority);
            }

            return prefabSchema;
        }

        private NetValueAuthorityObjectContainerSchema[] ExportNetValuesAuthoritySchemas(PositronNetworkIdentity[] prefabs)
        {
            List<NetValueAuthorityObjectContainerSchema> schema = new();

            for (ushort i = 0; i < prefabs.Length; i++)
            {
                INetValueManaged[] netValues = prefabs[i].GetAllNetValues();
                NetValueAuthoritySchema[] valueSchemas = new NetValueAuthoritySchema[netValues.Length];

                for (ushort j = 0; j < netValues.Length; j++)
                {
                    valueSchemas[j] = new(netValues[j].Authority);
                }

                schema.Add(new(valueSchemas));
            }

            return schema.ToArray();
        }
    }
}
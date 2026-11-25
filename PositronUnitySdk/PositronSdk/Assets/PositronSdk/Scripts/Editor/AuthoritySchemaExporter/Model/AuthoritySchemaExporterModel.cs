using Positron.Client.Mono;
using Positron.Client.NetValues;
using Positron.Client.Rpc;
using Positron.Editor.AuthoritySchemaExporter.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Positron.Editor.AuthoritySchemaExporter
{
    public sealed class AuthoritySchemaExporterModel
    {
        public AuthoritySchemaDto ExportObject(PositronNetworkIdentity[] prefabs, ulong[] rpcMethodNames, string[] rpcSourceNames)
        {
            PrefabAuthoritySchema[] prefabSchemas = ExportPrefabAuthoritySchemas(prefabs);
            NetValueAuthorityObjectContainerSchema[] netValuesSchemas = ExportNetValuesAuthoritySchemas(prefabs);
            RpcAuthoritySchema[] rpcAuthoritySchema = ExportRpcAuthoritySchema(prefabs, rpcMethodNames, rpcSourceNames);
            AuthoritySchemaDto combinedSchema = new(prefabSchemas, netValuesSchemas, rpcAuthoritySchema);

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
            List<NetValueAuthorityObjectContainerSchema> schema = new(prefabs.Length);

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

        private RpcAuthoritySchema[] ExportRpcAuthoritySchema(PositronNetworkIdentity[] prefabs, ulong[] rpcMethodNamesToCompare, string[] rpcSourceNames)
        {
            RpcAuthoritySchema[] schema = new RpcAuthoritySchema[rpcMethodNamesToCompare.Length];
            Dictionary<ulong, int> nameToPlainId = BuildComparisonTable(rpcMethodNamesToCompare);
            Dictionary<ulong, bool> warningFlags = BuildWarningFlags(rpcMethodNamesToCompare);

            foreach (PositronNetworkIdentity prefab in prefabs)
            {
                foreach (IRpcTarget rpcTarget in prefab.GetObservedRpcTargetsEditorOnlyNoSideEffects())
                {
                    foreach (RpcCallExportData rawData in rpcTarget.ExportRpcRawAuthoritySchema())
                    {
                        bool isNotContainedInTable = nameToPlainId.TryGetValue(rawData.RpcName, out int plaiId);

                        if (isNotContainedInTable && !warningFlags[rawData.RpcName])
                        {
                            schema[plaiId] = new(true, rawData.RpcTarget, rawData.RpcSource);
                            warningFlags[rawData.RpcName] = true;
                        }
                        else if (!isNotContainedInTable)
                        {
                            Debug.LogError($"unable to export {rawData.RpcSourceName} rpc due to it not contains in hash translation table");
                        }
                    }
                }
            }

            RaiseWarnings(warningFlags, rpcSourceNames);

            return schema;
        }

        private Dictionary<ulong, int> BuildComparisonTable(ulong[] rpcMethodNamesToCompare)
        {
            Dictionary<ulong, int> nameToPlainId = new(rpcMethodNamesToCompare.Length);

            for (int i = 0; i < rpcMethodNamesToCompare.Length; i++)
            {
                nameToPlainId.Add(rpcMethodNamesToCompare[i], i);
            }

            return nameToPlainId;
        }

        private Dictionary<ulong, bool> BuildWarningFlags(ulong[] rpcMethodNamesToCompare)
        {
            Dictionary<ulong, bool> alreadyExportedFlags = new(rpcMethodNamesToCompare.Length);

            for (int i = 0; i < rpcMethodNamesToCompare.Length; i++)
            {
                alreadyExportedFlags.Add(rpcMethodNamesToCompare[i], false);
            }

            return alreadyExportedFlags;
        }

        private void RaiseWarnings(Dictionary<ulong, bool> warningFlags, string[] rpcSourceNames)
        {
            int idx = 0;

            foreach (KeyValuePair<ulong, bool> kvp in warningFlags)
            {
                if (!kvp.Value)
                {
                    Debug.LogWarning($"Rpc '{rpcSourceNames[idx]}' declared but never used!");
                }

                idx++;
            }
        }
    }
}
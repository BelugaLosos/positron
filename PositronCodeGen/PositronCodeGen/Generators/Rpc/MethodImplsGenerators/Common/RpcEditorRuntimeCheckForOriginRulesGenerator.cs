using PositronCodeGen._Shared;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using System.Text;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common
{
    internal class RpcEditorRuntimeCheckForOriginRulesGenerator
    {
        public void GenerateCheck(StringBuilder str, ParsedMethodData data)
        {
            RpcSources source = (RpcSources)data.RpcAttr.ConstructorArguments[1].Value;

            str.AppendLine("#if UNITY_EDITOR"); // this is only for editor. too expensive for builds!
            if (source == RpcSources.Owner)
            {
                str.AppendLine($"        " +
                    $"if(!GetComponent<{ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}>()." +
                    $"{ConstantsHolderContainer.NETWORK_IDENTITY_IS_MINE_FIELD}) return;");
            }

            if (source == RpcSources.Host)
            {
                str.AppendLine($"        " +
                    $"if(!GetComponent<{ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}>()." +
                    $"{ConstantsHolderContainer.NETWORK_IDENTITY_IS_HOST_FIELD}) return;");
            }
            str.AppendLine("#endif");
        }
    }
}

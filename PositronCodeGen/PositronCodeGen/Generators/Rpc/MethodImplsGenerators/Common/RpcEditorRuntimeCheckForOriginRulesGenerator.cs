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

            if (source == RpcSources.All)
            {
                return;
            }

            str.AppendLine("#if UNITY_EDITOR"); // this is only for editor. too expensive for builds!
            if (source == RpcSources.Owner)
            {
                str.AppendLine($"        " +
                    $"if(!GetComponent<{ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}>()." +
                        $"{ConstantsHolderContainer.NETWORK_IDENTITY_IS_MINE_FIELD})");
                str.AppendLine("        {");
                str.AppendLine("            Debug.LogError(\"Positron debug err catched -> unable to call Owner rpc on foreign object!\", gameObject);");
                str.AppendLine("            return;");
                str.AppendLine("        }");
            }

            if (source == RpcSources.Host)
            {
                str.AppendLine($"        if({ConstantsHolderContainer.LOCAL_CLIENT_ID_GETTING_EXPR} != {ConstantsHolderContainer.HOST_CLIENT_ID_GETTING_EXPR})");
                str.AppendLine("        {");
                str.AppendLine("            Debug.LogError(\"Positron debug err catched -> unable to call Host rpc on non host object or not being a host!\", gameObject);");
                str.AppendLine("            return;");
                str.AppendLine("        }");
            }
            str.AppendLine("#endif");
        }
    }
}

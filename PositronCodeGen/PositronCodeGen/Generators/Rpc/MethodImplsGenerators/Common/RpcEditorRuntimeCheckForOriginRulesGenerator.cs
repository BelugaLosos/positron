using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using System.Text;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common
{
    internal class RpcEditorRuntimeCheckForOriginRulesGenerator
    {
        private const byte S_OWNER = 0x1;
        private const byte S_HOST = 0x2;

        public void GenerateCheck(StringBuilder str, ParsedMethodData data)
        {
            byte source = (byte)data.RpcAttr.ConstructorArguments[1].Value;

            str.AppendLine("#if UNITY_EDITOR"); // this is only for editor. too expensive for builds!
            if (source == S_OWNER)
            {
                str.AppendLine($"        " +
                    $"if(!GetComponent<{ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}>()." +
                    $"{ConstantsHolderContainer.NETWORK_IDENTITY_IS_MINE_FIELD}) return;");
            }

            if (source == S_HOST)
            {
                str.AppendLine($"        " +
                    $"if(!GetComponent<{ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}>()." +
                    $"{ConstantsHolderContainer.NETWORK_IDENTITY_IS_HOST_FIELD}) return;");
            }
            str.AppendLine("#endif");
        }
    }
}

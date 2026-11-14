using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Interfaces;
using System.Text;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators
{
    internal class RpcSpecifiedTargetPreditionImplGeneration : IRpcPreditionPartTargetDependentGenerator
    {
        public byte[] Targets => new byte[] { 0x2 }; // RPC_TARGET

        public void GenerateMethodImpl(StringBuilder str, ParsedMethodData data, RpcParamsDefinitionHelper.ExtractedParametersDefinitionsAsStrings paramsData)
        {
            string specifiedTargetParamName = string.Empty;

            foreach (ParsedMethodArgData arg in data.Args)
            {
                if (arg.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME)
                {
                    specifiedTargetParamName = arg.Name;
                }
            }

            string condition = $"{specifiedTargetParamName}.{ConstantsHolderContainer.RPC_SPECIFIED_TARGET_TARGET_FIELD_NAME} == " +
                $"{ConstantsHolderContainer.LOCAL_CLIENT_ID_GETTING_EXPR}";

            str.AppendLine($"        if({condition}) {data.MethodSymbol.Name}({paramsData.ParameterNames});");
        }
    }
}

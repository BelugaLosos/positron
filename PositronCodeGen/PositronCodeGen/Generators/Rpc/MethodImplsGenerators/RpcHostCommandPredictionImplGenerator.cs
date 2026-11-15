using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Interfaces;
using System.Text;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators
{
    internal class RpcHostCommandPredictionImplGenerator : IRpcPreditionPartTargetDependentGenerator
    {
        public byte[] Targets => new byte[] { 0x6 }; // RPC_HOST_COMMAND

        public void GenerateMethodImpl(StringBuilder str, ParsedMethodData data, RpcParamsDefinitionHelper.ExtractedParametersDefinitionsAsStrings paramsData)
        {
            str.AppendLine($"        if(isCurrentlyHost) {data.MethodSymbol.Name}({paramsData.ParameterNames});");
        }
    }
}

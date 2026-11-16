using PositronCodeGen._Shared;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Interfaces;
using System.Text;
using static PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common.RpcParamsDefinitionHelper;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators
{
    internal class RpcAllTargetPredictionImplGenerator : IRpcPreditionPartTargetDependentGenerator
    {
        public RpcTargets[] Targets => new RpcTargets[] { RpcTargets.RPC_ALL, RpcTargets.RPC_ALL_CACHED };

        public void GenerateMethodImpl(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings paramsData)
        {
            str.AppendLine($"        {data.MethodSymbol.Name}({paramsData.ParameterNames});");
        }
    }
}

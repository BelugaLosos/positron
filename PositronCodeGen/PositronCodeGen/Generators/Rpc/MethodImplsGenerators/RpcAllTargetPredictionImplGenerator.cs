using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Interfaces;
using System.Text;
using static PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common.RpcParamsDefinitionHelper;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators
{
    internal class RpcAllTargetPredictionImplGenerator : IRpcPreditionPartTargetDependentGenerator
    {
        public byte[] Targets => new byte[] { 0x0, 0x3 }; //ALL and ALL_CACHED

        public void GenerateMethodImpl(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings paramsData)
        {
            str.AppendLine($"        {data.MethodSymbol.Name}({paramsData.ParameterNames});");
        }
    }
}

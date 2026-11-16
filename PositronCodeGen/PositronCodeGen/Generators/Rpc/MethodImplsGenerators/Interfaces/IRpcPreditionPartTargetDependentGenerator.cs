using PositronCodeGen._Shared;
using PositronCodeGen.Extractors.Data;
using System.Text;
using static PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common.RpcParamsDefinitionHelper;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Interfaces
{
    internal interface IRpcPreditionPartTargetDependentGenerator
    {
        RpcTargets[] Targets { get; }
        void GenerateMethodImpl(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings paramsData);
    }
}

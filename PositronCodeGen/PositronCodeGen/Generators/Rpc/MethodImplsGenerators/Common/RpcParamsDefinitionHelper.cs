using PositronCodeGen.Extractors.Data;
using System.Text;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common
{
    internal class RpcParamsDefinitionHelper
    {
        public struct ExtractedParametersDefinitionsAsStrings
        {
            public string ParamsDefinition { get; private set; }
            public string ParameterNames { get; private set; }

            public ExtractedParametersDefinitionsAsStrings(string paramsDefinition, string parameterNames)
            {
                ParamsDefinition = paramsDefinition;
                ParameterNames = parameterNames;
            }
        }

        public ExtractedParametersDefinitionsAsStrings ExtractParamsDefinitionsAsStrings(ParsedMethodData data)
        {
            StringBuilder parametersDefinition = new StringBuilder("");
            StringBuilder parameterNames = new StringBuilder("");

            for (int i = 0; i < data.Args.Length; i++)
            {
                if (i != 0)
                {
                    parameterNames.Append(", ");
                    parametersDefinition.Append(", ");
                }

                parametersDefinition.Append(data.Args[i].DefinitionString);
                parametersDefinition.Append(" ");
                parametersDefinition.Append(data.Args[i].Name);
                parameterNames.Append(data.Args[i].Name);
            }

            return new ExtractedParametersDefinitionsAsStrings(parametersDefinition.ToString(), parameterNames.ToString());
        }
    }
}

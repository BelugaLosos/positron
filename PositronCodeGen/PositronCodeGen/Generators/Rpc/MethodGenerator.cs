using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common;
using PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Interfaces;
using PositronCodeGen.Util;
using System.Linq;
using System.Text;
using static PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common.RpcParamsDefinitionHelper;

namespace PositronCodeGen.Generators.Rpc
{
    internal class MethodGenerator
    {
        private readonly TypeDefinitionToIoMethodNameConverter _methodNamesConverter = new TypeDefinitionToIoMethodNameConverter();
        private readonly RpcWritePartGenerator _writePartGenerator;
        private readonly RpcParamsDefinitionHelper _paramsDefinitionHelper;
        private readonly IRpcPreditionPartTargetDependentGenerator[] _predictionPartsGenerators = new IRpcPreditionPartTargetDependentGenerator[]
        {
            new RpcAllTargetPredictionImplGenerator(),
            new RpcHostCommandPredictionImplGenerator(),
            new RpcSpecifiedTargetPreditionImplGeneration()
        };

        public MethodGenerator()
        {
            _writePartGenerator = new RpcWritePartGenerator(_methodNamesConverter);
            _paramsDefinitionHelper = new RpcParamsDefinitionHelper();
        }

        public void GenerateRWMethodsWithClosure(StringBuilder str, ParsedMethodData data)
        {
            ExtractedParametersDefinitionsAsStrings extractedParameters = _paramsDefinitionHelper.ExtractParamsDefinitionsAsStrings(data);

            GenerateWriteMethod(str, data, extractedParameters);
            GenerateReadMethod(str, data);
        }

        private void GenerateWriteMethod(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings extractedParameters)
        {
            byte target = (byte)data.RpcAttr.ConstructorArguments[0].Value;

            _writePartGenerator.Generate(str, data, extractedParameters, target);
            GeneratePredictionPart(str, data, extractedParameters, target);

            str.AppendLine("    }");
            str.AppendLine();
        }

        private void GeneratePredictionPart(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings extractedParams, byte target)
        {
            foreach (IRpcPreditionPartTargetDependentGenerator generator in _predictionPartsGenerators)
            {
                if (generator.Targets.Contains(target))
                {
                    generator.GenerateMethodImpl(str, data, extractedParams);
                    return;
                }
            }
        }

        private void GenerateReadMethod(StringBuilder str, ParsedMethodData data)
        {
            StringBuilder serviceReadCallParametersDefinition = new StringBuilder("");

            str.AppendLine($"    private void {ConstantsHolderContainer.RPC_READ_PREFIX}{data.MethodSymbol.Name}({ConstantsHolderContainer.NETWORK_READER_DEFINITION} reader)");
            str.AppendLine("    {");
            for (int i = 0; i < data.Args.Length; i++)
            {
                ParsedMethodArgData arg = data.Args[i];

                if (arg.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME)
                {
                    str.AppendLine($"        {ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME} p{i} = new(0);");
                }
                else
                {
                    str.AppendLine($"        {arg.DefinitionString} p{i} = reader.{_methodNamesConverter.ToReaderMethods(arg.DefinitionString)}();");
                }

                if (i == 0)
                {
                    serviceReadCallParametersDefinition.Append("p0");
                }
                else
                {
                    serviceReadCallParametersDefinition.Append($", p{i}");
                }
            }
            str.AppendLine();
            str.AppendLine($"        {data.MethodSymbol.Name}({serviceReadCallParametersDefinition});");
            str.AppendLine($"        {ConstantsHolderContainer.NETWORK_IO_POOL_DEFINITION}.{ConstantsHolderContainer.NETWORK_IO_POOL_PUT_READER_METHOD}(reader);");
            str.AppendLine("    }");

            str.AppendLine();
        }
    }
}

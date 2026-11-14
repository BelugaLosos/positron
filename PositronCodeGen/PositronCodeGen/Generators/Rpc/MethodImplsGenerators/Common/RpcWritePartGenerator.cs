using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System.Linq;
using System.Text;
using static PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common.RpcParamsDefinitionHelper;

namespace PositronCodeGen.Generators.Rpc.MethodImplsGenerators.Common
{
    internal class RpcWritePartGenerator
    {
        private readonly NamesHasher _namesHasher;
        private readonly TypeDefinitionToIoMethodNameConverter _typesNamesConverter;
        private readonly RpcEditorRuntimeCheckForOriginRulesGenerator _checkGenerator;

        private const byte RPC_HOST_COMMAND = 0x6;

        public RpcWritePartGenerator(TypeDefinitionToIoMethodNameConverter typesNameConverter)
        {
            _namesHasher = new NamesHasher();
            _typesNamesConverter = typesNameConverter;
        }

        public void Generate(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings paramsData, byte target)
        {
            string methodName = data.MethodSymbol.Name;
            ulong hash = _namesHasher.To64BitHash(methodName);

            GenerateMethodSignatureAndBeginning(str, data, paramsData.ParamsDefinition, methodName);
            str.AppendLine("    {");
            _checkGenerator.GenerateCheck(str, data);
            str.AppendLine("        bool exceptionRaised = false;");
            
            ConditionalGenerateHostCommandLogicExpr(str, target);

            AddSpace(str);

            ConditionalGerateIfStatementOfWritePart(str, target);

            str.AppendLine("        {");
            str.AppendLine("            try");
            str.AppendLine("            {");

            GenerateWritePart(str, data);
            AddSpace(str);
            GenerateInternalsCall(str, data, hash, target);

            str.AppendLine("            }");
            str.AppendLine("            catch(global::System.Exception e)");
            str.AppendLine("            {");
            str.AppendLine("                exceptionRaised = true;");
            str.AppendLine("                Debug.LogException(e);");
            str.AppendLine("            }");
            str.AppendLine("        }");
            AddSpace(str);
            str.AppendLine("        if(exceptionRaised)");
            str.AppendLine("        {");
            str.AppendLine("            return;");
            str.AppendLine("        }");
            AddSpace(str);
        }

        private void AddSpace(StringBuilder str) => str.AppendLine();

        private void GenerateMethodSignatureAndBeginning(StringBuilder str, ParsedMethodData data, string paramsDefinition, string methodName)
        {
            string accessModifier = AccessDeclarationToStringConverter.AccesebilityDeclarationToString(data.MethodSymbol.DeclaredAccessibility);
            str.AppendLine($"    {accessModifier} void {ConstantsHolderContainer.RPC_SEND_PREFIX}{methodName}({paramsDefinition})");
        }

        private void ConditionalGenerateHostCommandLogicExpr(StringBuilder str, byte target)
        {
            if (target == RPC_HOST_COMMAND)
            {
                str.AppendLine($"        bool isCurrentlyHost = {ConstantsHolderContainer.LOCAL_CLIENT_ID_GETTING_EXPR} == {ConstantsHolderContainer.HOST_CLIENT_ID_GETTING_EXPR};");
            }
        }

        private void ConditionalGerateIfStatementOfWritePart(StringBuilder str, byte target)
        {
            if (target == RPC_HOST_COMMAND)
            {
                str.AppendLine($"        if(!{ConstantsHolderContainer.RPCS_MODEL_DEFINITION}.IsInRollbufferMode && !isCurrentlyHost)");
            }
            else
            {
                str.AppendLine($"        if(!{ConstantsHolderContainer.RPCS_MODEL_DEFINITION}.IsInRollbufferMode)");
            }
        }

        private void GenerateWritePart(StringBuilder str, ParsedMethodData data)
        {
            str.AppendLine($"                {ConstantsHolderContainer.NETWORK_WRITER_DEFINITION} w = {ConstantsHolderContainer.NETWORK_IO_POOL_DEFINITION}.{ConstantsHolderContainer.NETWORK_IO_POOL_GET_WRITTER_METHOD}();");
            str.AppendLine();
            for (int i = 0; i < data.Args.Length; i++)
            {
                ParsedMethodArgData arg = data.Args[i];

                if (arg.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME)
                {
                    continue;
                }

                str.AppendLine($"                w.{_typesNamesConverter.ToWriterMethods(arg.DefinitionString)}({arg.Name});");
            }
        }

        private void GenerateInternalsCall(StringBuilder str, ParsedMethodData data, ulong methodNameHash, byte target) =>
            str.AppendLine($"                " +
                $"{ConstantsHolderContainer.NETWORK_RPCS_MODEL_SEND_TO_SERVER_METHOD}(" +
                $"this, " +
                $"gameObject, " +
                $"{methodNameHash}, " +
                $"{GenerateTargetClientSpecifiedParams(data)}, " +
                $"({ConstantsHolderContainer.RPC_TARGETS_ENUM_DEFINITION}){target}, w);");

        private string GenerateTargetClientSpecifiedParams(ParsedMethodData data) => data.Args.
                Where(a => a.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME).Any() ?
                $"{data.Args.Where(a => a.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME).First().Name}.TargetClientId, true" :
                "0, false";
    }
}

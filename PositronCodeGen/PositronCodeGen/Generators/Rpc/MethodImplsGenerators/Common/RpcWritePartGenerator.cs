using PositronCodeGen._Shared;
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

        public RpcWritePartGenerator(TypeDefinitionToIoMethodNameConverter typesNameConverter)
        {
            _namesHasher = new NamesHasher();
            _typesNamesConverter = typesNameConverter;
            _checkGenerator = new RpcEditorRuntimeCheckForOriginRulesGenerator();
        }

        public void Generate(StringBuilder str, ParsedMethodData data, ExtractedParametersDefinitionsAsStrings paramsData, RpcTargets target)
        {
            string methodName = data.MethodSymbol.Name;
            ulong hash = _namesHasher.To64BitHash(methodName);
            int specifiedTargetParamIdx = FindSpecifiedTargetArg(data, out string specifiedTargetParamName);

            GenerateMethodSignatureAndBeginning(str, data, paramsData.ParamsDefinition, methodName);
            str.AppendLine("    {");
            _checkGenerator.GenerateCheck(str, data);
            str.AppendLine("        bool exceptionRaised = false;");
            
            ConditionalGenerateHostCommandLogicExpr(str, target);

            AddSpace(str);

            ConditionalGerateIfStatementOfWritePart(str, target, specifiedTargetParamName);

            str.AppendLine("        {");
            str.AppendLine($"            {ConstantsHolderContainer.NETWORK_WRITER_DEFINITION} w = {ConstantsHolderContainer.NETWORK_IO_POOL_DEFINITION}.{ConstantsHolderContainer.NETWORK_IO_POOL_GET_WRITTER_METHOD}();");
            
            AddSpace(str);

            str.AppendLine("            try");
            str.AppendLine("            {");

            GenerateWritePart(str, data, specifiedTargetParamIdx);
            AddSpace(str);
            GenerateInternalsCall(str, data, hash, target);

            str.AppendLine("            }");
            str.AppendLine("            catch(global::System.Exception e)");
            str.AppendLine("            {");
            str.AppendLine("                exceptionRaised = true;");
            str.AppendLine("                Debug.LogException(e);");
            str.AppendLine("            }");
            AddSpace(str);
            str.AppendLine("            if(exceptionRaised)");
            str.AppendLine("            {");
            str.AppendLine($"                {ConstantsHolderContainer.NETWORK_IO_POOL_PUT_WRITER_METHOD_CALL}(w);");
            str.AppendLine("                return;");
            str.AppendLine("            }");
            str.AppendLine("        }");
            AddSpace(str);
        }

        private void AddSpace(StringBuilder str) => str.AppendLine();

        private void GenerateMethodSignatureAndBeginning(StringBuilder str, ParsedMethodData data, string paramsDefinition, string methodName)
        {
            string accessModifier = AccessDeclarationToStringConverter.AccesebilityDeclarationToString(data.MethodSymbol.DeclaredAccessibility);
            str.AppendLine($"    {accessModifier} void {ConstantsHolderContainer.RPC_SEND_PREFIX}{methodName}({paramsDefinition})");
        }

        private void ConditionalGenerateHostCommandLogicExpr(StringBuilder str, RpcTargets target)
        {
            if (target == RpcTargets.RPC_HOST_COMMAND)
            {
                str.AppendLine($"        bool isCurrentlyHost = {ConstantsHolderContainer.LOCAL_CLIENT_ID_GETTING_EXPR} == {ConstantsHolderContainer.HOST_CLIENT_ID_GETTING_EXPR};");
            }
        }

        private void ConditionalGerateIfStatementOfWritePart(StringBuilder str, RpcTargets target, string specifiedParamName)
        {
            string notInRollBufferModeExpr = $"!{ConstantsHolderContainer.RPCS_MODEL_DEFINITION}.IsInRollbufferMode";

            if (target == RpcTargets.RPC_HOST_COMMAND)
            {
                str.AppendLine($"        if({notInRollBufferModeExpr} && !isCurrentlyHost)");
            }
            else if (target == RpcTargets.RPC_TARGET)
            {
                str.AppendLine($"        if({notInRollBufferModeExpr} && " +
                    $"{specifiedParamName}.{ConstantsHolderContainer.RPC_SPECIFIED_TARGET_TARGET_FIELD_NAME} != " +
                    $"{ConstantsHolderContainer.LOCAL_CLIENT_ID_GETTING_EXPR})");
            }
            else
            {
                str.AppendLine($"        if({notInRollBufferModeExpr})");
            }
        }

        private int FindSpecifiedTargetArg(ParsedMethodData data, out string name)
        {
            for (int i = 0; i < data.Args.Length; i++)
            {
                ParsedMethodArgData arg = data.Args[i];

                if (arg.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME)
                {
                    name = arg.Name;
                    return i;
                }
            }

            name = string.Empty;
            return -1;
        }

        private void GenerateWritePart(StringBuilder str, ParsedMethodData data, int specifiedTargetArgIndex)
        {
            for (int i = 0; i < data.Args.Length; i++)
            {
                ParsedMethodArgData arg = data.Args[i];

                if (specifiedTargetArgIndex == i)
                {
                    continue;
                }

                str.AppendLine($"                w.{_typesNamesConverter.ToWriterMethods(arg.DefinitionString)}({arg.Name});");
            }
        }

        private void GenerateInternalsCall(StringBuilder str, ParsedMethodData data, ulong methodNameHash, RpcTargets target) =>
            str.AppendLine($"                " +
                $"{ConstantsHolderContainer.NETWORK_RPCS_MODEL_SEND_TO_SERVER_METHOD}(" +
                $"this, " +
                $"gameObject, " +
                $"{methodNameHash}, " +
                $"{GenerateTargetClientSpecifiedParams(data)}, " +
                $"{ConstantsHolderContainer.RPC_TARGETS_ENUM_DEFINITION}.{target}, w);");

        private string GenerateTargetClientSpecifiedParams(ParsedMethodData data) => data.Args.
                Where(a => a.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME).Any() ?
                $"{data.Args.Where(a => a.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME).First().Name}.TargetClientId, true" :
                "0, false";
    }
}

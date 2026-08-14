using Microsoft.CodeAnalysis;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System;

namespace PositronCodeGen.Generators
{
    internal class MethodGenerator
    {
        private readonly TypeDefinitionToIoMethodNameConverter _typesNamesConverter;
        private readonly NamesHasher _namesHasher;

        public MethodGenerator()
        {
            _typesNamesConverter = new TypeDefinitionToIoMethodNameConverter();
            _namesHasher = new NamesHasher();   
        }

        public void GenerateMethodWithClosure(StringBuilder str, ParsedMethodData data)
        {
            string accessModifier = AccessDeclarationToStringConverter.AccesebilityDeclarationToString(data.MethodSymbol.DeclaredAccessibility);

            StringBuilder parametersDefinition = new StringBuilder("");
            StringBuilder parameterNames = new StringBuilder("");
            StringBuilder serviceReadCallParametersDefinition = new StringBuilder("");

            string specifiedTargetCall = data.Args.
                Where(a => a.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME).Any() ?
                $"{data.Args.Where(a => a.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME).First().Name}.TargetClientId, true" :
                "0, false";

            string methodName = data.MethodSymbol.Name;
            ulong hash = _namesHasher.To64BitHash(methodName);

            for (int i = 0; i < data.Args.Length; i++)
            {
                if (i != 0)
                {
                    parameterNames.Append(", ");
                }

                parametersDefinition.Append(", ");
                parametersDefinition.Append(data.Args[i].DefinitionString);
                parametersDefinition.Append(" ");
                parametersDefinition.Append(data.Args[i].Name);
                parameterNames.Append(data.Args[i].Name);
            }

            str.AppendLine($"    {accessModifier} void {ConstantsHolderContainer.RPC_SEND_PREFIX}{methodName}({ConstantsHolderContainer.RPC_TARGETS_ENUM_DEFINITION} targets{parametersDefinition})");
            str.AppendLine("    {");
            str.AppendLine("        bool exceptionRaised = false;");
            str.AppendLine();
            str.AppendLine($"        if(!{ConstantsHolderContainer.RPCS_MODEL_DEFINITION}.IsInRollbufferMode)");
            str.AppendLine("        {");
            str.AppendLine("            try");
            str.AppendLine("            {");
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
            str.AppendLine();
            str.AppendLine($"                {ConstantsHolderContainer.NETWORK_RPCS_MODEL_SEND_TO_SERVER_METHOD}(this, gameObject, {hash}, {specifiedTargetCall}, targets, w);");
            str.AppendLine("            }");
            str.AppendLine("            catch(global::System.Exception e)");
            str.AppendLine("            {");
            str.AppendLine("                exceptionRaised = true;");
            str.AppendLine("                Debug.LogException(e);");
            str.AppendLine("            }");
            str.AppendLine("        }");
            str.AppendLine();
            str.AppendLine("        if(exceptionRaised)");
            str.AppendLine("        {");
            str.AppendLine("            return;");
            str.AppendLine("        }");
            str.AppendLine();
            str.AppendLine($"        if(targets == {ConstantsHolderContainer.RPC_TARGETS_ENUM_DEFINITION}.{ConstantsHolderContainer.RPC_TARGETS_ENUM_VALUE_RPC_ALL} || targets == {ConstantsHolderContainer.RPC_TARGETS_ENUM_DEFINITION}.{ConstantsHolderContainer.RPC_TARGETS_ENUM_VALUE_RPC_ALL_CACHED})");
            str.AppendLine("        {");
            str.AppendLine($"            {methodName}({parameterNames});");
            str.AppendLine("        }");
            str.AppendLine("    }");
            str.AppendLine();

            str.AppendLine($"    private void {ConstantsHolderContainer.RPC_READ_PREFIX}{methodName}({ConstantsHolderContainer.NETWORK_READER_DEFINITION} reader)");
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
                    str.AppendLine($"        {arg.DefinitionString} p{i} = reader.{_typesNamesConverter.ToReaderMethods(arg.DefinitionString)}();");
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
            str.AppendLine($"        {methodName}({serviceReadCallParametersDefinition});");
            str.AppendLine($"        {ConstantsHolderContainer.NETWORK_IO_POOL_DEFINITION}.{ConstantsHolderContainer.NETWORK_IO_POOL_PUT_READER_METHOD}(reader);");
            str.AppendLine("    }");

            str.AppendLine();
        }
    }
}

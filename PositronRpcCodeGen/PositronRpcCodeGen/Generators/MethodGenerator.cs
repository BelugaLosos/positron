using Microsoft.CodeAnalysis;
using PositronRpcCodeGen.Extractors.Data;
using PositronRpcCodeGen.Util;
using System.Text;

namespace PositronRpcCodeGen.Generators
{
    internal class MethodGenerator
    {
        private readonly TypeDefinitionToIoMethodNameConverter _typesNamesConverter;

        public MethodGenerator()
        {
            _typesNamesConverter = new TypeDefinitionToIoMethodNameConverter();
        }

        public void GenerateMethodWithClosure(StringBuilder str, ParsedMethodData data)
        {
            Accessibility accesebility = data.MethodSymbol.DeclaredAccessibility;
            string accessModifier = "private";

            switch (accesebility)
            {
                case Accessibility.Public:
                    accessModifier = "public";
                    break;

                case Accessibility.Protected:
                    accessModifier = "protected";
                    break;

                case Accessibility.Private:
                    accessModifier = "private";
                    break;
            }

            StringBuilder parametersDefinition = new StringBuilder("");
            StringBuilder serviceReadCallParametersDefinition = new StringBuilder("");

            foreach (ParsedMethodArgData arg in data.Args)
            {
                parametersDefinition.Append(", ");
                parametersDefinition.Append(arg.DefinitionString);
                parametersDefinition.Append(" ");
                parametersDefinition.Append(arg.Name);
            }

            str.AppendLine($"    {accessModifier} void SendRPC_{data.MethodSymbol.Name}(global::Positron.Client.ConstantHolders.RpcTargets targets{parametersDefinition})");
            str.AppendLine("    {");
            str.AppendLine("        global::Positron.NetworkIoAPI.PositronNetworkWriter w = global::Positron.PositronFacade.NetworkIoPool.GetWriter();");
            for (int i = 0; i < data.Args.Length; i++)
            {
                ParsedMethodArgData arg = data.Args[i];
                str.AppendLine($"        w.{_typesNamesConverter.ToWriterMethods(arg.DefinitionString)}({arg.Name});");
            }
            str.AppendLine($"        global::Positron.PositronFacade.World.RpcModel.SendRpcToServer(this, \"{data.MethodSymbol.Name}\", 0, targets, w);");
            str.AppendLine("    }");

            str.AppendLine();

            str.AppendLine($"    private void CODEGEN_SERVICE_METHOD_ReadRPC_{data.MethodSymbol.Name}(global::Positron.NetworkIoAPI.PositronNetworkReader reader)");
            str.AppendLine("    {");
            for (int i = 0; i < data.Args.Length; i++)
            {
                ParsedMethodArgData arg = data.Args[i];
                str.AppendLine($"        {arg.DefinitionString} p{i} = reader.{_typesNamesConverter.ToReaderMethods(arg.DefinitionString)}();");

                if (i == 0)
                {
                    serviceReadCallParametersDefinition.Append("p0");
                }
                else
                {
                    serviceReadCallParametersDefinition.Append($", p{i}");
                }
            }
            str.AppendLine($"        {data.MethodSymbol.Name}({serviceReadCallParametersDefinition});");
            str.AppendLine("        global::Positron.PositronFacade.NetworkIoPool.PutReader(reader);");
            str.AppendLine("    }");

            str.AppendLine();
        }
    }
}

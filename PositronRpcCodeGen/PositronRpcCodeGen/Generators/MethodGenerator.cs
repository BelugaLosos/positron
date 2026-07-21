using Microsoft.CodeAnalysis;
using PositronRpcCodeGen.Extractors.Data;
using System.Text;

namespace PositronRpcCodeGen.Generators
{
    internal class MethodGenerator
    {
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

            foreach (ParsedMethodArgData arg in data.Args)
            {
                parametersDefinition.Append(", ");
                parametersDefinition.Append(arg.DefinitionString);
                parametersDefinition.Append(" ");
                parametersDefinition.Append(arg.Name);
            }

            str.AppendLine($"    {accessModifier} void SendRPC_{data.MethodSymbol.Name}(global::Positron.Client.ConstantHolders.RpcTargets targets{parametersDefinition})");

            str.AppendLine("    {");
            str.AppendLine("        Debug.Log(\"Hello world\");");
            str.AppendLine("    }");

            str.AppendLine();
        }
    }
}

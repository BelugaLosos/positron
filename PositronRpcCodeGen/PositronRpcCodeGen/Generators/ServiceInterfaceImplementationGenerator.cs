using PositronRpcCodeGen.ConstantsHolder;
using PositronRpcCodeGen.Extractors.Data;
using System.Text;

namespace PositronRpcCodeGen.Generators
{
    internal class ServiceInterfaceImplementationGenerator
    {
        public void GenerateInterfaceImplementationAccordingTo(StringBuilder str, ParsedMethodData[] methods)
        {
            str.AppendLine($"    public bool {ConstantsHolderContainer.SUITABILITY_METHOD_DEFINITION}(string name)");
            str.AppendLine("    {");
            str.AppendLine("        switch (name)");
            str.AppendLine("        {");
            foreach (ParsedMethodData method in methods)
            {
            str.AppendLine($"            case \"{method.MethodSymbol.Name}\": return true;");
            }
            str.AppendLine("        }");
            str.AppendLine("        return false;");
            str.AppendLine("    }");


            str.AppendLine();


            str.AppendLine($"    public void {ConstantsHolderContainer.CALL_METHOD_DEFINITION}(string name, {ConstantsHolderContainer.NETWORK_READER_DEFINITION} reader)");
            str.AppendLine("    {");
            str.AppendLine("        switch (name)");
            str.AppendLine("        {");
            foreach (ParsedMethodData method in methods)
            {
                str.AppendLine($"            case \"{method.MethodSymbol.Name}\": {ConstantsHolderContainer.RPC_READ_PREFIX}{method.MethodSymbol.Name}(reader); break;");
            }
            str.AppendLine("        }");
            str.AppendLine("    }");
            str.AppendLine();
        }
    }
}

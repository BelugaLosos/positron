using PositronRpcCodeGen.ConstantsHolder;
using PositronRpcCodeGen.Extractors.Data;
using PositronRpcCodeGen.Util;
using System.Text;

namespace PositronRpcCodeGen.Generators
{
    internal class ServiceInterfaceImplementationGenerator
    {
        private readonly NamesHasher _namesHasher;

        public ServiceInterfaceImplementationGenerator()
        {
            _namesHasher = new NamesHasher();
        }

        public void GenerateInterfaceImplementationAccordingTo(StringBuilder str, ParsedMethodData[] methods)
        {
            str.AppendLine($"    public bool {ConstantsHolderContainer.SUITABILITY_METHOD_DEFINITION}(ulong name)");
            str.AppendLine("    {");
            str.AppendLine("        switch (name)");
            str.AppendLine("        {");
            foreach (ParsedMethodData method in methods)
            {
                ulong methodNameHash = _namesHasher.To64BitHash(method.MethodSymbol.Name);
                str.AppendLine($"            case {methodNameHash}: return true;");
            }
            str.AppendLine("        }");
            str.AppendLine("        return false;");
            str.AppendLine("    }");


            str.AppendLine();


            str.AppendLine($"    public void {ConstantsHolderContainer.CALL_METHOD_DEFINITION}(ulong name, {ConstantsHolderContainer.NETWORK_READER_DEFINITION} reader)");
            str.AppendLine("    {");
            str.AppendLine("        switch (name)");
            str.AppendLine("        {");
            foreach (ParsedMethodData method in methods)
            {
                ulong methodNameHash = _namesHasher.To64BitHash(method.MethodSymbol.Name);
                str.AppendLine($"            case {methodNameHash}: {ConstantsHolderContainer.RPC_READ_PREFIX}{method.MethodSymbol.Name}(reader); break;");
            }
            str.AppendLine("        }");
            str.AppendLine("    }");
            str.AppendLine();
        }
    }
}

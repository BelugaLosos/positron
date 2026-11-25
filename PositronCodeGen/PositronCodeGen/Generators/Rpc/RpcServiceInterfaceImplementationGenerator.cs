using PositronCodeGen._Shared;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System.Collections.Generic;
using System.Text;

namespace PositronCodeGen.Generators.Rpc
{
    internal class RpcServiceInterfaceImplementationGenerator
    {
        private readonly NamesHasher _namesHasher;
        private readonly List<ulong> _nameHashesCache;

        public RpcServiceInterfaceImplementationGenerator()
        {
            _namesHasher = new NamesHasher();
            _nameHashesCache = new List<ulong>(16);   
        }

        public void GenerateInterfaceImplementationAccordingTo(StringBuilder str, ParsedMethodData[] methods)
        {
            _nameHashesCache.Clear();

            str.AppendLine($"    public bool {ConstantsHolderContainer.SUITABILITY_METHOD_DEFINITION}(ulong name)");
            str.AppendLine("    {");
            str.AppendLine("        switch (name)");
            str.AppendLine("        {");
            foreach (ParsedMethodData method in methods)
            {
                ulong methodNameHash = _namesHasher.To64BitHash(method.MethodSymbol.Name);
                _nameHashesCache.Add(methodNameHash);
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

            str.AppendLine("#if UNITY_EDITOR");
            str.AppendLine($"    public {ConstantsHolderContainer.RPC_RAW_AUTHORITY_SCHEMA_EXPORT_DATA}[] {ConstantsHolderContainer.RPC_RAW_AUTHORITY_SCHEMA_EXPORT_METHOD_NAME}()");
            str.AppendLine("    {");
            str.AppendLine($"        {ConstantsHolderContainer.RPC_RAW_AUTHORITY_SCHEMA_EXPORT_DATA}[] d = new {ConstantsHolderContainer.RPC_RAW_AUTHORITY_SCHEMA_EXPORT_DATA}[{methods.Length}];");
            str.AppendLine();
            for (int i = 0; i < methods.Length; i++)
            {
                RpcTargets targets = (RpcTargets)methods[i].RpcAttr.ConstructorArguments[0].Value;
                RpcSources sources = (RpcSources)methods[i].RpcAttr.ConstructorArguments[1].Value;

                str.AppendLine($"        d[{i}] = new(\"{methods[i].MethodSymbol.Name}\", {_nameHashesCache[i]}, {ConstantsHolderContainer.RPC_TARGETS_ENUM_DEFINITION}.{targets}, {ConstantsHolderContainer.RPC_SOURCES_ENUM_DEFINITION}.{sources});");
            }
            str.AppendLine();
            str.AppendLine("        return d;");
            str.AppendLine("    }");
            str.AppendLine("#endif");

            str.AppendLine();
        }
    }
}

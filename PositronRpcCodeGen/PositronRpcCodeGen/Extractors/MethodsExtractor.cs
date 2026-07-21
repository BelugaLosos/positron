using Microsoft.CodeAnalysis;
using PositronRpcCodeGen.Extractors.Data;
using System.Collections.Generic;
using System.Linq;

namespace PositronRpcCodeGen.Extractors
{
    internal class MethodsExtractor
    {
        public IEnumerable<ParsedMethodData> ExtractMethodsFromType(INamedTypeSymbol type, string attributeClassName)
        {
            foreach (IMethodSymbol method in type.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind == MethodKind.Ordinary)
                {
                    AttributeData attrData = method.
                        GetAttributes().
                        Where(a => a.AttributeClass?.Name == attributeClassName).
                        FirstOrDefault();

                    if (attrData == null)
                    {
                        continue;
                    }

                    yield return new ParsedMethodData(method, attrData, GetMethodArgs(method));
                }
            }
        }

        private ParsedMethodArgData[] GetMethodArgs(IMethodSymbol method)
        {
            List<ParsedMethodArgData> args = new List<ParsedMethodArgData>();

            foreach (IParameterSymbol param in method.Parameters)
            {
                string name = param.Name;
                string def = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                IParameterSymbol sym = param;

                args.Add(new ParsedMethodArgData(name, def, sym));
            }   

            return args.ToArray();
        }
    }
}
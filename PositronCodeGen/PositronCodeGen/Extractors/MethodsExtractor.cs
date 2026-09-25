using Microsoft.CodeAnalysis;
using PositronCodeGen.Extractors.Data;
using System.Collections.Generic;
using System.Linq;

namespace PositronCodeGen.Extractors
{
    internal class MethodsExtractor
    {
        public List<ParsedMethodData> ExtractMethodsFromType(INamedTypeSymbol type, string attributeClassName)
        {
            List<ParsedMethodData> res = new List<ParsedMethodData>();

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

                    res.Add(new ParsedMethodData(method, attrData, GetMethodArgs(method)));
                }
            }

            return res;
        }

        public ParsedMethodArgData[] GetMethodArgs(IMethodSymbol method)
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
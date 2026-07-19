using Microsoft.CodeAnalysis;
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

                    yield return new ParsedMethodData(method, attrData);
                }
            }
        }
    }
}
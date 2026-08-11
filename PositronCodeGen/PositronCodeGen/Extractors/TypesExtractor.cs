using Microsoft.CodeAnalysis;
using PositronCodeGen.Extractors.Data;
using System.Collections.Generic;

namespace PositronCodeGen.Extractors
{
    internal class TypesExtractor
    {
        public IEnumerable<ParsedTypeData> ExtractAllTypesFromAssembly(Compilation compiler, TypeKind kind) =>
            DoExtractTypes(compiler.GlobalNamespace, kind);

        private IEnumerable<ParsedTypeData> DoExtractTypes(INamespaceSymbol space, TypeKind kind)
        {
            foreach (INamedTypeSymbol type in space.GetTypeMembers())
            {
                if (!type.IsImplicitlyDeclared && type.TypeKind == kind)
                {
                    yield return new ParsedTypeData(type, type.ContainingNamespace);
                }
            }

            foreach (INamespaceSymbol nestedNamespace in space.GetNamespaceMembers())
            {
                foreach (ParsedTypeData typeFromNested in DoExtractTypes(nestedNamespace, kind))
                {
                    yield return typeFromNested;
                }
            }
        }
    }
}
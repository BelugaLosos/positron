using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PositronRpcCodeGen.Extractors
{
    internal class TypesExtractor
    {
        public IEnumerable<INamedTypeSymbol> ExtractAllTypesFromAssembly(Compilation compiler, TypeKind kind) =>
            DoExtractTypes(compiler.GlobalNamespace, kind);

        private IEnumerable<INamedTypeSymbol> DoExtractTypes(INamespaceSymbol space, TypeKind kind)
        {
            foreach (INamedTypeSymbol type in space.GetTypeMembers())
            {
                if (!type.IsImplicitlyDeclared && type.TypeKind == kind)
                {
                    yield return type;
                }
            }

            foreach (INamespaceSymbol nestedNamespace in space.GetNamespaceMembers())
            {
                foreach (INamedTypeSymbol typeFromNested in DoExtractTypes(nestedNamespace, kind))
                {
                    yield return typeFromNested;
                }
            }
        }
    }
}
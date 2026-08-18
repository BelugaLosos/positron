using Microsoft.CodeAnalysis;

namespace PositronCodeGen.Extractors.Data
{
    internal struct ParsedTypeData
    {
        public INamedTypeSymbol Type;
        public INamespaceSymbol NameSpace;

        public ParsedTypeData(INamedTypeSymbol type, INamespaceSymbol nameSpace)
        {
            Type = type;
            NameSpace = nameSpace;
        }

        public string GetNamespaceName()
        {
            SymbolDisplayFormat NamespaceFormat = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
            );

            return NameSpace.ToDisplayString(NamespaceFormat);
        }
    }
}

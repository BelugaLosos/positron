using Microsoft.CodeAnalysis;

namespace PositronRpcCodeGen.Extractors.Data
{
    internal struct ParsedMethodArgData
    {
        public string Name;
        public string DefinitionString;
        public IParameterSymbol SourceSymbol;

        public ParsedMethodArgData(string name, string defString, IParameterSymbol sourceSymbol)
        {
            Name = name;
            DefinitionString = defString;
            SourceSymbol = sourceSymbol;
        }
    }
}

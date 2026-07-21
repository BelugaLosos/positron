using Microsoft.CodeAnalysis;

namespace PositronRpcCodeGen.Extractors.Data
{
    internal struct ParsedMethodData
    {
        public IMethodSymbol MethodSymbol;
        public AttributeData RpcAttr;
        public ParsedMethodArgData[] Args;

        public ParsedMethodData(IMethodSymbol method, AttributeData attr, ParsedMethodArgData[] args)
        {
            MethodSymbol = method;
            RpcAttr = attr;
            Args = args;
        }
    }
}

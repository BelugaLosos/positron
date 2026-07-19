using Microsoft.CodeAnalysis;

namespace PositronRpcCodeGen.Extractors
{
    internal struct ParsedMethodData
    {
        public IMethodSymbol MethodSymbol;
        public AttributeData RpcAttr;

        public ParsedMethodData(IMethodSymbol method, AttributeData attr)
        {
            MethodSymbol = method;
            RpcAttr = attr;
        }
    }
}

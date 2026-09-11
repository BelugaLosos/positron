using Microsoft.CodeAnalysis;
using PositronCodeGen.Extractors.Data;
using System.Text;

namespace PositronCodeGen.Processors.Interface
{
    internal interface ITypeProcessor
    {
        void Process(ParsedTypeData type, GeneratorExecutionContext context, StringBuilder sourceBuilder);
    }
}

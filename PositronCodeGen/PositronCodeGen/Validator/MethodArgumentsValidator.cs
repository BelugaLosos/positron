using Microsoft.CodeAnalysis;
using PositronCodeGen.Extractors;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System.Linq;

namespace PositronCodeGen.Validator
{
    internal class MethodArgumentsValidator
    {
        private MethodsExtractor _argumentsExtractor;

        public MethodArgumentsValidator()
        {
            _argumentsExtractor = new MethodsExtractor();
        }

        public bool IsArgumentsValid(INamedTypeSymbol owner, IMethodSymbol method, GeneratorExecutionContext context)
        {
            ParsedMethodArgData[] args = _argumentsExtractor.GetMethodArgs(method);

            foreach (ParsedMethodArgData arg in args)
            {
                TypeKind tk = arg.SourceSymbol.Type.TypeKind;
                string defStr = arg.DefinitionString;

                if (tk == TypeKind.Class && defStr != "string")
                {
                    context.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport(
                            owner, 
                            $"ivalid types in RPC {defStr}", 
                            $"ivalid types in RPC {defStr}", 
                            method.Locations.FirstOrDefault())
                        );

                    return false;
                }

                if (tk != TypeKind.Array && tk != TypeKind.Struct && tk != TypeKind.Structure && tk != TypeKind.Array && tk != TypeKind.Class)
                {
                    context.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport(
                            owner,
                            $"ivalid types (type kind) in RPC {tk}",
                            $"ivalid types (type kind) in RPC {tk}",
                            method.Locations.FirstOrDefault())
                        );

                    return false;
                }
            }

            return true;
        }
    }
}

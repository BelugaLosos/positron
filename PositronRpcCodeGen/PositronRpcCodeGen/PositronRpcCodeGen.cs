using Microsoft.CodeAnalysis;
using PositronRpcCodeGen.Extractors;
using PositronRpcCodeGen.Validator;
using System.Linq;

namespace PositronRpcCodeGen
{
    [Generator]
    public class PositronRpcCodeGen : ISourceGenerator
    {
        private const string RPC_ATTR_NAME = "RpcAttribute";

        public void Execute(GeneratorExecutionContext context)
        {
            Compilation compiler = context.Compilation;

            TypesExtractor typesExtractor = new TypesExtractor();
            MethodsExtractor methodsExtractor = new MethodsExtractor();
            ClassPartialsValidator partialsValidator = new ClassPartialsValidator();

            foreach (INamedTypeSymbol type in typesExtractor.ExtractAllTypesFromAssembly(compiler, TypeKind.Class))
            {
                foreach (ParsedMethodData method in methodsExtractor.ExtractMethodsFromType(type, RPC_ATTR_NAME))
                {
                    if (!partialsValidator.ClassIsPartial(type))
                    {
                        Diagnostic diagnosticsReport = Diagnostic.Create(
                                partialsValidator.GenerateDiagnosticsDescriptor(type),
                                type.Locations.FirstOrDefault() ?? Location.None,
                                type.Name
                            );

                        context.ReportDiagnostic(diagnosticsReport);

                        return;
                    }
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}

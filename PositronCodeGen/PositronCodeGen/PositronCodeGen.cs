using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators;
using PositronCodeGen.Validator;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PositronCodeGen
{
    [Generator]
    public class PositronCodeGen : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Compilation compiler = context.Compilation;
            StringBuilder sourceBuilder = new StringBuilder();

            TypesExtractor typesExtractor = new TypesExtractor();
            MethodsExtractor methodsExtractor = new MethodsExtractor();

            ClassDeclarationValidator partialsValidator = new ClassDeclarationValidator();
            MethodValidator methodValidator = new MethodValidator();

            UsagesGenerator usagesGenerator = new UsagesGenerator();
            ClassGenerator classGenerator = new ClassGenerator();
            MethodGenerator methodGenerator = new MethodGenerator();
            ServiceInterfaceImplementationGenerator serviceCodeGenerator = new ServiceInterfaceImplementationGenerator();

            usagesGenerator.GenerateUsages(sourceBuilder);

            foreach (ParsedTypeData type in typesExtractor.ExtractAllTypesFromAssembly(compiler, TypeKind.Class))
            {
                List<ParsedMethodData> methods = methodsExtractor.ExtractMethodsFromType(type.Type, ConstantsHolderContainer.RPC_ATTR_NAME).ToList();

                if (!partialsValidator.ClassIsDeclaredCorrectly(type.Type) && methods.Count > 0)
                {
                    Diagnostic diagnosticsReport = Diagnostic.Create(
                            partialsValidator.GenerateDiagnosticsDescriptor(),
                            type.Type.Locations.FirstOrDefault() ?? Location.None,
                            type.Type.Name
                        );

                    context.ReportDiagnostic(diagnosticsReport);

                    return;
                }
                
                if (methods.Count == 0)
                {  
                    continue;
                }
                
                classGenerator.AppendInitial(sourceBuilder, type.Type.DeclaredAccessibility, type.Type.IsSealed, type.Type.Name, type.GetNamespaceName());
                serviceCodeGenerator.GenerateInterfaceImplementationAccordingTo(sourceBuilder, methods.ToArray());

                foreach (ParsedMethodData method in methods)
                {
                    if (!methodValidator.IsMethodValid(method.MethodSymbol, out string message))
                    {
                        Diagnostic diagnosticsReport = Diagnostic.Create(
                               methodValidator.GenerateDiagnosticsDescriptor(),
                               method.MethodSymbol.Locations.FirstOrDefault() ?? Location.None,
                               message
                           );

                        context.ReportDiagnostic(diagnosticsReport);

                        return;
                    }

                    methodGenerator.GenerateMethodWithClosure(sourceBuilder, method);
                }

                classGenerator.AppendClosure(sourceBuilder, type.NameSpace.ToDisplayString());
            }

            context.AddSource("RpcLowLevelInteractors.gen.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}

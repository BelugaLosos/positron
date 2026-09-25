using Microsoft.CodeAnalysis;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators;
using PositronCodeGen.Generators.Rpc;
using PositronCodeGen.Processors.Interface;
using PositronCodeGen.Validator;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PositronCodeGen.Processors
{
    internal sealed class RpcIntegrationProcessor : ITypeProcessor
    {
        private readonly MethodsExtractor _methodsExtractor;
        private readonly ClassDeclarationValidator _partialsValidator;
        private readonly MethodValidator _methodValidator;
        private readonly MethodArgumentsValidator _methodArgumentsValidator;
        private readonly ClassGenerator _classGenerator;
        private readonly MethodGenerator _methodGenerator;
        private readonly ServiceInterfaceImplementationGenerator _serviceCodeGenerator;

        public RpcIntegrationProcessor()
        {
            _methodsExtractor = new MethodsExtractor();
            _partialsValidator = new ClassDeclarationValidator();
            _methodValidator = new MethodValidator();
            _methodArgumentsValidator = new MethodArgumentsValidator();
            _classGenerator = new ClassGenerator();
            _methodGenerator = new MethodGenerator();
            _serviceCodeGenerator = new ServiceInterfaceImplementationGenerator();
        }

        public void Process(ParsedTypeData type, GeneratorExecutionContext context, StringBuilder sourceBuilder)
        {
            List<ParsedMethodData> methods = _methodsExtractor.ExtractMethodsFromType(type.Type, ConstantsHolderContainer.RPC_ATTR_NAME);

            if (!_partialsValidator.ClassIsDeclaredCorrectly(type.Type) && methods.Count > 0)
            {
                _partialsValidator.ReportDiagnosticClassDeclaration(context, type.Type);
                return;
            }

            if (methods.Count == 0)
            {
                return;
            }

            foreach (ParsedMethodData method in methods)
            {
                if (!_methodValidator.IsMethodValid(method.MethodSymbol, out string message))
                {
                    Diagnostic diagnosticsReport = Diagnostic.Create(
                           _methodValidator.GenerateDiagnosticsDescriptor(),
                           method.MethodSymbol.Locations.FirstOrDefault() ?? Location.None,
                           message
                    );
                    context.ReportDiagnostic(diagnosticsReport);

                    return;
                }

                if (!_methodArgumentsValidator.IsArgumentsValid(type.Type, method.MethodSymbol, context))
                {
                    return;
                }
            }

            _classGenerator.AppendInitial(sourceBuilder, type, "RPCs", ConstantsHolderContainer.RPC_TARGETS_INTERFACE_DEFINITION);

            _serviceCodeGenerator.GenerateInterfaceImplementationAccordingTo(sourceBuilder, methods.ToArray());

            foreach (ParsedMethodData method in methods)
            {
                _methodGenerator.GenerateMethodWithClosure(sourceBuilder, method);
            }

            _classGenerator.AppendClosure(sourceBuilder, type);
        }
    }
}

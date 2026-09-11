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
        private readonly ClassGenerator _classGenerator;
        private readonly MethodGenerator _methodGenerator;
        private readonly ServiceInterfaceImplementationGenerator _serviceCodeGenerator;

        public RpcIntegrationProcessor()
        {
            _methodsExtractor = new MethodsExtractor();
            _partialsValidator = new ClassDeclarationValidator();
            _methodValidator = new MethodValidator();
            _classGenerator = new ClassGenerator();
            _methodGenerator = new MethodGenerator();
            _serviceCodeGenerator = new ServiceInterfaceImplementationGenerator();
        }

        public void Process(ParsedTypeData type, GeneratorExecutionContext context, StringBuilder sourceBuilder)
        {
            List<ParsedMethodData> methods = _methodsExtractor.ExtractMethodsFromType(type.Type, ConstantsHolderContainer.RPC_ATTR_NAME).ToList();

            if (!_partialsValidator.ClassIsDeclaredCorrectly(type.Type) && methods.Count > 0)
            {
                _partialsValidator.ReportDiagnostic(context, type.Type);
                return;
            }

            if (methods.Count == 0)
            {
                return;
            }

            _classGenerator.AppendInitial(sourceBuilder, type, "RPCs", ConstantsHolderContainer.RPC_TARGETS_INTERFACE_DEFINITION);

            _serviceCodeGenerator.GenerateInterfaceImplementationAccordingTo(sourceBuilder, methods.ToArray());

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

                _methodGenerator.GenerateMethodWithClosure(sourceBuilder, method);
            }

            _classGenerator.AppendClosure(sourceBuilder, type);
        }
    }
}

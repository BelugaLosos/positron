using Microsoft.CodeAnalysis;
using PositronCodeGen._Shared;
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
        private readonly ClassDeclarationValidator _classValidator;
        private readonly MethodModificatorsValidator _methodModificatorsValidator;
        private readonly MethodArgumentsValidator _methodArgumentsValidator;
        private readonly RpcArgumentsToSourceValidatorCopatebility _arguemntsToTargetsCopatebilityValidator;
        private readonly RpcTargetSpecArgToNonTargetTargetSpecifierValidator _specArgumentToOtherTargetsValidator;
        private readonly ClassGenerator _classGenerator;
        private readonly RpcMethodsGenerator _methodGenerator;
        private readonly ServiceInterfaceImplementationGenerator _serviceCodeGenerator;

        public RpcIntegrationProcessor()
        {
            _methodsExtractor = new MethodsExtractor();
            _classValidator = new ClassDeclarationValidator();
            _methodModificatorsValidator = new MethodModificatorsValidator();
            _methodArgumentsValidator = new MethodArgumentsValidator();
            _classGenerator = new ClassGenerator();
            _methodGenerator = new RpcMethodsGenerator();
            _serviceCodeGenerator = new ServiceInterfaceImplementationGenerator();
            _arguemntsToTargetsCopatebilityValidator = new RpcArgumentsToSourceValidatorCopatebility();
            _specArgumentToOtherTargetsValidator = new RpcTargetSpecArgToNonTargetTargetSpecifierValidator();
        }

        public void Process(ParsedTypeData type, GeneratorExecutionContext context, StringBuilder sourceBuilder)
        {
            List<ParsedMethodData> methods = _methodsExtractor.ExtractMethodsFromType(type.Type, ConstantsHolderContainer.RPC_ATTR_NAME);

            if (!_classValidator.ClassIsDeclaredCorrectly(type.Type) && methods.Count > 0)
            {
                _classValidator.ReportDiagnosticClassDeclaration(context, type.Type);
                return;
            }

            if (methods.Count == 0)
            {
                return;
            }

            foreach (ParsedMethodData method in methods)
            {
                if (!_methodModificatorsValidator.IsMethodValid(method.MethodSymbol, out string message))
                {
                    Diagnostic diagnosticsReport = Diagnostic.Create(
                           _methodModificatorsValidator.GenerateDiagnosticsDescriptor(),
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

                RpcTargets rpcTarget = (RpcTargets)method.RpcAttr.ConstructorArguments[0].Value;

                if (!_arguemntsToTargetsCopatebilityValidator.IsValid(type.Type, rpcTarget, method, context))
                {
                    return;
                }

                if (!_specArgumentToOtherTargetsValidator.IsValid(type.Type, rpcTarget, method, context))
                {
                    return;
                }
            }

            _classGenerator.AppendInitial(sourceBuilder, type, "RPCs", ConstantsHolderContainer.RPC_TARGETS_INTERFACE_DEFINITION);

            _serviceCodeGenerator.GenerateInterfaceImplementationAccordingTo(sourceBuilder, methods.ToArray());

            foreach (ParsedMethodData method in methods)
            {
                _methodGenerator.GenerateRWMethodsWithClosure(sourceBuilder, method);
            }

            _classGenerator.AppendClosure(sourceBuilder, type);
        }
    }
}

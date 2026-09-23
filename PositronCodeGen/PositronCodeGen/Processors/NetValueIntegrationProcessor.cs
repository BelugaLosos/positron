using Microsoft.CodeAnalysis;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators;
using PositronCodeGen.Generators.NetValues;
using PositronCodeGen.Processors.Interface;
using PositronCodeGen.Validator;
using System.Collections.Generic;
using System.Text;

namespace PositronCodeGen.Processors
{
    internal sealed class NetValueIntegrationProcessor : ITypeProcessor
    {
        private readonly ClassDeclarationValidator _classValidator;
        private readonly NetworkedFieldsValidator _netValuesFieldsValidator;
        private readonly FieldsExtractor _fieldsExtractor;
        private readonly InitializationInterfaceImplementation _impelementGenerator;
        private readonly ClassGenerator _classGenerator;
        
        public NetValueIntegrationProcessor()
        {
            _classValidator = new ClassDeclarationValidator();
            _netValuesFieldsValidator = new NetworkedFieldsValidator();
            _fieldsExtractor = new FieldsExtractor();
            _impelementGenerator = new InitializationInterfaceImplementation();
            _classGenerator = new ClassGenerator();
        }

        public void Process(ParsedTypeData type, GeneratorExecutionContext context, StringBuilder sourceBuilder)
        {
            List<FieldData> fields = _fieldsExtractor.ExtractFieldsData(type, ConstantsHolderContainer.NET_VALUE_ATTR_NAME);

            if (fields.Count == 0)
            {
                return;
            }

            if (!_classValidator.ClassIsDeclaredCorrectly(type.Type))
            {
                _classValidator.ReportDiagnosticClassDeclaration(context, type.Type);
                return;
            }

            if (!_netValuesFieldsValidator.IsFieldsSectionValid(fields.ToArray(), context))
            {
                return;
            }

            if (_classValidator.IsClassImplementsInterface(type.Type, ConstantsHolderContainer.NET_VALUE_CARRIER_DEFINITION))
            {
                _classValidator.ReportDiagnosticsClassInterfaceImplementation(context, type.Type, ConstantsHolderContainer.NET_VALUE_CARRIER_DEFINITION);
                return;
            }
           
            _classGenerator.AppendInitial(sourceBuilder, type, "NetValues", ConstantsHolderContainer.NET_VALUE_CARRIER_DEFINITION);
            _impelementGenerator.GenerateMethodImplementationFromNetValueInterface(sourceBuilder, fields);
            _classGenerator.AppendClosure(sourceBuilder, type);
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PositronCodeGen.Extractors.Data;
using System.Collections.Generic;
using System.Linq;

namespace PositronCodeGen.Extractors
{
    internal class FieldsExtractor
    {
        public List<FieldData> ExtractFieldsData(ParsedTypeData type, string targetDefinition)
        {
            List<FieldData> fields = new List<FieldData>(); 

            foreach(IFieldSymbol field in type.Type.GetMembers().OfType<IFieldSymbol>())
            {
                AttributeData attrData = field.GetAttributes().Where(a => a.AttributeClass?.Name == targetDefinition).FirstOrDefault();

                if (attrData != null) 
                {
                    SyntaxReference synRef = field.DeclaringSyntaxReferences.FirstOrDefault();
                    bool hasDefault = false;

                    if (synRef != null)
                    {
                        VariableDeclaratorSyntax declarationSyntax = synRef.GetSyntax() as VariableDeclaratorSyntax;
                        hasDefault = declarationSyntax.Initializer != null;
                    }

                    fields.Add(new FieldData(field.Name, attrData, hasDefault));
                }
            }

            return fields;
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PositronCodeGen.Extractors.Data;
using System.Linq;

namespace PositronCodeGen.Validator
{
    internal class ClassDeclarationValidator
    {
        public bool ClassIsDeclaredCorrectly(INamedTypeSymbol type)
        {
            if (type.TypeKind != TypeKind.Class)
            {
                return false;
            }

            foreach (SyntaxReference syntaxRef in type.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax() is ClassDeclarationSyntax classDeclaraction)
                {
                    if (classDeclaraction.Modifiers.Where(m => m.ValueText == "abstract" || m.ValueText == "static").Any())
                    {
                        return false;
                    }

                    if (classDeclaraction.Modifiers.Where(m => m.ValueText == "partial").Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ReportDiagnostic(GeneratorExecutionContext context, INamedTypeSymbol type)
        {
            context.ReportDiagnostic(GeneratReport(type));
        }

        private Diagnostic GeneratReport(INamedTypeSymbol type) => Diagnostic.Create(
                                                                    GenerateDiagnosticsDescriptor(),
                                                                    type.Locations.FirstOrDefault() ?? Location.None,
                                                                    type.Name
                                                               );

        private DiagnosticDescriptor GenerateDiagnosticsDescriptor()
        {
            return new DiagnosticDescriptor(
                    "Positron code gen",
                    "Class (only Classes supported) with RPCs or NetValues (Networked attr) must be declared as PARTIAL, not STATIC and not ABSTRACT !!!",
                    "Mapped code {0} must be declared as PARTIAL, not STATIC and not ABSTRACT class!!!",
                    "Positron codegen report",
                    DiagnosticSeverity.Error,
                    true
                );
        }
    }
}
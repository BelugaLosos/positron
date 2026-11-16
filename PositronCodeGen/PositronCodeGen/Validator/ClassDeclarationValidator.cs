using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PositronCodeGen.Util;
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

                    if (classDeclaraction.Modifiers.Where(m => m.ValueText == "partial").Any() && 
                        classDeclaraction.Modifiers.Where(n => n.ValueText == "sealed").Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsClassImplementsInterface(INamedTypeSymbol type, string interfaceName) =>
            type.
            AllInterfaces.
            Where(i => i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == interfaceName).
            Any();

        public void ReportDiagnosticClassDeclaration(GeneratorExecutionContext context, INamedTypeSymbol type) => 
            context.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport(
                    type,
                    "Class (only Classes supported) with RPCs or NetValues (Networked attr) must be declared as PARTIAL and SEALED, not STATIC and not ABSTRACT !!!",
                    "Mapped code {0} must be declared as PARTIAL, not STATIC and not ABSTRACT class!!!"
        ));

        public void ReportDiagnosticsClassInterfaceImplementation(GeneratorExecutionContext context, INamedTypeSymbol type, string interfaceName) => 
            context.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport(type, 
                $"This class can not implement {interfaceName}",
                $"This class can not implement {interfaceName}")
        );
    }
}
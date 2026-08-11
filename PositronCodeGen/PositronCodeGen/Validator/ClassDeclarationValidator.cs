using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public DiagnosticDescriptor GenerateDiagnosticsDescriptor()
        {
            return new DiagnosticDescriptor(
                    "RPC Codegen",
                    "Class (only Classes supported) with RPCs must be declared as PARTIAL, not STATIC and not ABSTRACT !!!",
                    "Rpc-constaining code {0} must be declared as PARTIAL, not STATIC and not ABSTRACT class!!!",
                    "Positron codegen report",
                    DiagnosticSeverity.Error,
                    true
                );
        }
    }
}
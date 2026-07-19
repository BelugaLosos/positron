using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace PositronRpcCodeGen.Validator
{
    internal class ClassPartialsValidator
    {
        public bool ClassIsPartial(INamedTypeSymbol type)
        {
            if (type.TypeKind != TypeKind.Class)
            {
                return false;
            }

            foreach (SyntaxReference syntaxRef in type.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax() is ClassDeclarationSyntax classDeclaraction)
                {
                    if (classDeclaraction.Modifiers.Where(m => m.ValueText == "partial").Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public DiagnosticDescriptor GenerateDiagnosticsDescriptor(INamedTypeSymbol type)
        {
            return new DiagnosticDescriptor(
                    "RPC Codegen " + Guid.NewGuid().ToString(),
                    "Class with RPCs must be declared as PARTIAL !!!",
                    "Class {0} must be declared as PARTIAL !!!",
                    "Positron codegen report",
                    DiagnosticSeverity.Error,
                    true
                );
        }
    }
}
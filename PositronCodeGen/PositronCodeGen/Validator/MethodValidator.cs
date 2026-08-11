using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace PositronCodeGen.Validator
{
    internal class MethodValidator
    {
        private readonly List<Accessibility> _allowedAccesebilities = new List<Accessibility>();

        public MethodValidator() 
        {
            _allowedAccesebilities.Add(Accessibility.Private);
            _allowedAccesebilities.Add(Accessibility.Protected);
            _allowedAccesebilities.Add(Accessibility.Public);
        }

        public bool IsMethodValid(IMethodSymbol method, out string message)
        {
            if (method.IsVirtual)
            {
                message = "Rpc method can`t be virtual";
                return false;
            }

            if (method.IsStatic)
            {
                message = "Rpc method can`t be static";
                return false;
            }

            if (!_allowedAccesebilities.Contains(method.DeclaredAccessibility))
            {
                message = $"Rpc method can`t be {method.DeclaredAccessibility} (Access type)";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public DiagnosticDescriptor GenerateDiagnosticsDescriptor()
        {
            return new DiagnosticDescriptor(
                    "RPC Codegen " + Guid.NewGuid().ToString(),
                    "Rpc method invalid declaration",
                    "{0}",
                    "Positron codegen report [Method]",
                    DiagnosticSeverity.Error,
                    true
                );
        }
    }
}

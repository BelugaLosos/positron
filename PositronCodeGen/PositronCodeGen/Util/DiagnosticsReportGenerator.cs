using Microsoft.CodeAnalysis;
using System.Linq;

namespace PositronCodeGen.Util
{
    internal static class DiagnosticsReportGenerator
    {
        public static Diagnostic GeneratReport(INamedTypeSymbol type, string title, string description, Location locOverride = null) => Diagnostic.Create(
                                                                    GenerateDiagnosticsDescriptor(title, description),
                                                                    locOverride ?? type.Locations.FirstOrDefault() ?? Location.None,
                                                                    type.Name
                                                               );

        private static DiagnosticDescriptor GenerateDiagnosticsDescriptor(string title, string description)
        {
            return new DiagnosticDescriptor(
                    "Positron code gen",
                    title,
                    description,
                    "Positron codegen report",
                    DiagnosticSeverity.Error,
                    true
                );
        }
    }
}

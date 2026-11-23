using Microsoft.CodeAnalysis;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Util;
using System.Linq;

namespace PositronCodeGen.Validator
{
    internal class ClassInheritanceToMonoBehaviourValidator
    {
        public bool IsValid(INamedTypeSymbol type, GeneratorExecutionContext ctx)
        {
            INamedTypeSymbol nextNamedType = type.BaseType;
            bool isSeenMonoBehaviour = false;

            while (nextNamedType != null)
            {
                if (nextNamedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ConstantsHolderContainer.UNITY_MONO_BEHAVIOUR_DEF_STRING)
                {
                    isSeenMonoBehaviour = true;
                    break;
                }

                nextNamedType = nextNamedType.BaseType;
            }

            if (!isSeenMonoBehaviour)
            {
                ctx.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport
                    (
                        type,
                        "class thatrealises network interactions (RPC or NetValue) must be mono behaviour",
                        "class thatrealises network interactions (RPC or NetValue) must be mono behaviour",
                        type.Locations.FirstOrDefault()
                   ));
            }

            return isSeenMonoBehaviour;
        }
    }
}

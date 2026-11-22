using Microsoft.CodeAnalysis;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System.Linq;
using PositronCodeGen._Shared;

namespace PositronCodeGen.Validator
{
    /// <summary>
    /// checks is method with RPC_TARGET contains RpcPlayerRef in args
    /// </summary>
    internal class RpcArgumentsToSourceValidatorCopatebility
    {
        public bool IsValid(INamedTypeSymbol owner, RpcTargets target, ParsedMethodData data, GeneratorExecutionContext ctx)
        {
            if (target != RpcTargets.RPC_TARGET)
            {
                return true;
            }

            foreach (ParsedMethodArgData arg in data.Args)
            {
                if (arg.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME)
                {
                    return true;
                }
            }

            ctx.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport(
                    owner,
                    "Rpc incompatible target and argumens. Add RpcPlayerRef to arguments if you want to use Rpc Targeted",
                    "Rpc incompatible target and argumens. Add RpcPlayerRef to arguments if you want to use Rpc Targeted",
                    data.MethodSymbol.Locations.FirstOrDefault()
                ));

            return false;
        }
    }
}

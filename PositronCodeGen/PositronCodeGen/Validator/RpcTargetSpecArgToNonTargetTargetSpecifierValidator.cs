using Microsoft.CodeAnalysis;
using PositronCodeGen._Shared;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System.Linq;

namespace PositronCodeGen.Validator
{
    /// <summary>
    /// validates this rule 
    ///     (targets == RpcTargets.RPC_ALL || targets == RpcTargets.RPC_ALL_CACHED ||
    ///     targets == RpcTargets.RPC_OTHERS || targets == RpcTargets.RPC_OTHERS_CACHED) && hasSpecifiedTarget --> invalid/fail
    /// </summary>
    internal class RpcTargetSpecArgToNonTargetTargetSpecifierValidator
    {
        public bool IsValid(INamedTypeSymbol owner, RpcTargets target, ParsedMethodData data, GeneratorExecutionContext ctx)
        {
            bool hasTargetCorrectTarget = target == RpcTargets.RPC_ALL || target == RpcTargets.RPC_ALL_CACHED ||
                                          target == RpcTargets.RPC_OTHERS || target == RpcTargets.RPC_OTHERS_CACHED;

            bool hasSpecifiedTarget = false;

            foreach (ParsedMethodArgData arg in data.Args)
            {
                if (arg.DefinitionString == ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME)
                {
                    hasSpecifiedTarget = true;
                    break;
                }
            }

            if (hasTargetCorrectTarget && hasSpecifiedTarget)
            {
                ctx.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport
                    (
                        owner, 
                        $"Rpc with target that not is {nameof(RpcTargets.RPC_TARGET)} can`t have {ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME}",
                        $"Rpc with target that not is {nameof(RpcTargets.RPC_TARGET)} can`t have {ConstantsHolderContainer.RPC_SPECIFIED_TARGET_STRUCT_NAME}",
                        data.MethodSymbol.Locations.FirstOrDefault()
                    ));

                return false;
            }

            return true;
        }
    }
}

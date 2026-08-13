using PositronCodeGen.ConstantsHolder;
using Microsoft.CodeAnalysis;
using System.Text;
using PositronCodeGen.Util;

namespace PositronCodeGen.Generators
{
    internal class ClassGenerator
    {
        public void AppendInitial(StringBuilder str, Accessibility accessModifier, bool isSealed, string className, string namespaceName)
        {
            string accessModifierString = AccessDeclarationToStringConverter.AccesebilityDeclarationToString(accessModifier);
            string sealedMod = " ";

            str.AppendLine($"\n\n//Generated encoders for RPCs in this file (DO NOT TOUCH AND EDIT BY HANDS)");

            if (isSealed)
            {
                sealedMod = " sealed ";
            }

            if (!string.IsNullOrEmpty(namespaceName))
            {
                str.AppendLine($"namespace {namespaceName}");
                str.AppendLine("{");
            }

            str.AppendLine($"[RequireComponent(typeof({ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}))]");
            str.AppendLine($"{accessModifierString}{sealedMod}partial class {className} : {ConstantsHolderContainer.RPC_TARGETS_INTERFACE_DEFINITION}");
            str.AppendLine("{");
        }

        public void AppendClosure(StringBuilder str, string namespaceName)
        {
            str.AppendLine("}");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                str.AppendLine("}");
            }

            str.AppendLine("//Generation ends (DO NOT TOUCH AND EDIT BY HANDS) \n\n");
        }
    }
}

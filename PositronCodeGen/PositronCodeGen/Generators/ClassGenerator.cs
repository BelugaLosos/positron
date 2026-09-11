using PositronCodeGen.ConstantsHolder;
using Microsoft.CodeAnalysis;
using System.Text;
using PositronCodeGen.Util;
using PositronCodeGen.Extractors.Data;

namespace PositronCodeGen.Generators
{
    internal class ClassGenerator
    {
        public void AppendInitial(StringBuilder str, ParsedTypeData type, string purpose, string implenetationDefinitionString)
        {
            string accessModifierString = AccessDeclarationToStringConverter.AccesebilityDeclarationToString(type.Type.DeclaredAccessibility);
            string sealedMod = " ";

            str.AppendLine($"\n\n//Generated encoders for {purpose} in this file (DO NOT TOUCH AND EDIT BY HANDS)");

            if (type.Type.IsSealed)
            {
                sealedMod = " sealed ";
            }

            if (!string.IsNullOrEmpty(type.GetNamespaceName()))
            {
                str.AppendLine($"namespace {type.GetNamespaceName()}");
                str.AppendLine("{");
            }

            str.AppendLine($"[RequireComponent(typeof({ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}))]");
            str.AppendLine($"{accessModifierString}{sealedMod}partial class {type.Type.Name} : {implenetationDefinitionString}");
            str.AppendLine("{");
        }

        public void AppendClosure(StringBuilder str, ParsedTypeData type)
        {
            str.AppendLine("}");

            if (!string.IsNullOrEmpty(type.GetNamespaceName()))
            {
                str.AppendLine("}");
            }

            str.AppendLine("//Generation ends (DO NOT TOUCH AND EDIT BY HANDS) \n\n");
        }
    }
}

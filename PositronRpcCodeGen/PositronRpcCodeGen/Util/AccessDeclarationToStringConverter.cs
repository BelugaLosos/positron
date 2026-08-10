using Microsoft.CodeAnalysis;

namespace PositronRpcCodeGen.Util
{
    internal static class AccessDeclarationToStringConverter
    {
        public static string AccesebilityDeclarationToString(Accessibility accesebility)
        {
            string accessModifier = "private";

            switch (accesebility)
            {
                case Accessibility.Public:
                    accessModifier = "public";
                    break;

                case Accessibility.Protected:
                    accessModifier = "protected";
                    break;

                case Accessibility.Private:
                    accessModifier = "private";
                    break;
                case Accessibility.Internal:
                    accessModifier = "internal";
                    break;
            }

            return accessModifier;
        }
    }
}

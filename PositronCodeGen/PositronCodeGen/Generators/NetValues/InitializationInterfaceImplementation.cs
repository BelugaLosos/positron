using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using System.Collections.Generic;
using System.Text;

namespace PositronCodeGen.Generators.NetValues
{
    internal sealed class InitializationInterfaceImplementation
    {
        public void GenerateMethodImplementationFromNetValueInterface(StringBuilder sourceBuilder, List<FieldData> fields)
        {
            StringBuilder returnDefSb = new StringBuilder();

            sourceBuilder.AppendLine($"    public {ConstantsHolderContainer.NET_VALUE_MANGED_DEFINITION}[] {ConstantsHolderContainer.NET_VALUE_CARRIER_GET_VALUES_METHOD_DEFINITION}()");
            sourceBuilder.AppendLine("    {");
            sourceBuilder.AppendLine($"         {ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION} n = gameObject.GetComponent<{ConstantsHolderContainer.POSITRON_NETWORK_IDENTITY_DEFINITION}>();");
            sourceBuilder.AppendLine();

            foreach (FieldData field in fields)
            {
                if (!field.HasDefaultInit)
                {
                    sourceBuilder.AppendLine($"        {field.Name} = new();");
                }

                bool hasPredictionFlag = false;

                foreach(var argument in field.Attribute.ConstructorArguments)
                {
                    foreach (var attrParam in field.Attribute.AttributeConstructor.Parameters)
                    {
                        if (attrParam.Name == ConstantsHolderContainer.NET_VALUE_PREDICTABLE_FLAG_NAME && argument.Value is bool isPredictable && isPredictable) 
                        {
                            hasPredictionFlag = true;
                            break;
                        }
                    }

                    if (hasPredictionFlag)
                    {
                        break;
                    }
                }

                sourceBuilder.AppendLine($"        {field.Name}.{ConstantsHolderContainer.NET_VALUE_BIND_NETWORK_OBJECT_METHOD_DEFINITION}(n, {(hasPredictionFlag ? "true" : "false")});");
                returnDefSb.Append($"{field.Name}, ");
            }

            sourceBuilder.AppendLine();

            string returnDef = returnDefSb.ToString();
            sourceBuilder.AppendLine($"        return new {ConstantsHolderContainer.NET_VALUE_MANGED_DEFINITION}[] {{{returnDef.Substring(0, returnDef.Length - 2)}}};");

            sourceBuilder.AppendLine("    }");
        }
    }
}

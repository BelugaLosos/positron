using Microsoft.CodeAnalysis;
using PositronCodeGen.ConstantsHolder;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PositronCodeGen.Validator
{
    internal class NetworkedFieldsValidator
    {
        public Dictionary<string, string> _forbiddenTypes = new Dictionary<string, string>
        {
            { "int", "IntNetValue" },
            { "uint", "UintNetValue" },
            { "short", "ShortNetValue" },
            { "ushort", "UshortNetValue" },
            { "long", "LongNetValue" },
            { "ulong", "UlongNetValue" },
            { "global::UnityEngine.Vector2", "Vector2NetValue" },
            { "global::UnityEngine.Vector3", "Vector2NetValue" },
            { "global::UnityEngine.Vector2Int", "Vector2IntNetValue" },
            { "global::UnityEngine.Vector3Int", "Vector3IntNetValue" },
            { "global::UnityEngine.Vector4", "Vector3NetValue" },
            { "global::UnityEngine.Quaternion", "QuaternionNetValue" },
            { "float", "FloatNetValue" },
            { "double", "DoubleNetValue" },
            { "byte", "ByteNetValue" },
            { "sbyte", "SbyteNetValue" },
            { "bool", "BoolNetValue" },
        };

        public bool IsFieldsSectionValid(ReadOnlySpan<FieldData> fields, GeneratorExecutionContext context)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (!IsFieldValid(fields[i], out string message))
                {
                    context.ReportDiagnostic(DiagnosticsReportGenerator.GeneratReport(
                            fields[i].Type as INamedTypeSymbol, 
                            "invalid usage of net values",
                            message,
                            fields[i].Locations.FirstOrDefault()
                        ));

                    return false;
                }
            }

            return true;
        }

        private bool IsFieldValid(FieldData field, out string message)
        {
            if (field.Type.IsStatic)
            {
                message = "networked field can not be static";
                return false;
            }

            if (field.Type.BaseType == null)
            {
                message = $"networked field can be only with type of Positron`s pool. any custom type must implement {ConstantsHolderContainer.NET_VALUE_BASR_CLASS_TYPE_NAME} or use ComplexNetValue<T> instead";
                return false;
            }

            string name = field.Type.BaseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            bool isBaseTypeGeneric = field.Type.BaseType.IsGenericType;

            if ((isBaseTypeGeneric && name.Split('<')[0] != ConstantsHolderContainer.NET_VALUE_BASR_CLASS_TYPE_NAME) ||
                (!isBaseTypeGeneric && name != ConstantsHolderContainer.NET_VALUE_BASR_CLASS_TYPE_NAME))
            {
                message = $"networked field can be only with type of Positron`s pool. any custom type must implement {ConstantsHolderContainer.NET_VALUE_BASR_CLASS_TYPE_NAME} or use ComplexNetValue<T> instead. current base type is {name}";
                return false;
            }

            if (field.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                List<string> typeNames = namedType.TypeArguments.Select(t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToList();

                foreach(KeyValuePair<string, string> forbidden in _forbiddenTypes)
                {
                    if (typeNames.Contains(forbidden.Key))
                    {
                        message = $"in field by name {field.Name} type usage of complex values is invalid use {forbidden.Value} instead";
                        return false;
                    }
                }
            }

            message = string.Empty;
            return true;
        }
    }
}

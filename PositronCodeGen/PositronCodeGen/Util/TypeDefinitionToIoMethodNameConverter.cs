namespace PositronCodeGen.Util
{
    internal class TypeDefinitionToIoMethodNameConverter
    {
        public string ToWriterMethods(string definitionString)
        {
            switch (definitionString)
            {
                case "long": return "WriteLong";
                case "int": return "WriteInt";
                case "short": return "WriteShort";
                case "ulong": return "WriteUlong";
                case "uint": return "WriteUint";
                case "ushort": return "WriteUshort";
                case "byte": return "WriteByte";
                case "sbyte": return "WriteSbyte";
                case "bool": return "WriteBool";
                case "float": return "WriteFloat";
                case "double": return "WriteDouble";
                case "byte[]": return "WriteBytes";
                case "string": return "WriteString";
                default: return $"WriteComplexObject<{definitionString}>";
            }
        }

        public string ToReaderMethods(string definitionString)
        {
            switch (definitionString)
            {
                case "long": return "ReadLong";
                case "int": return "ReadInt";
                case "short": return "ReadShort";
                case "ulong": return "ReadUlong";
                case "uint": return "ReadUint";
                case "ushort": return "ReadUshort";
                case "byte": return "ReadByte";
                case "sbyte": return "ReadSbyte";
                case "bool": return "ReadBool";
                case "float": return "ReadFloat";
                case "double": return "ReadDouble";
                case "byte[]": return "ReadBytes.ToArray";
                case "string": return "ReadString";
                default: return $"ReadComplex<{definitionString}>";
            }
        }
    }
}

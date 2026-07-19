using System.Text;

namespace PositronRpcCodeGen.Generators
{
    internal class ClassGenerator
    {
        public void AppendInitial(StringBuilder str, string className)
        {
            str.AppendLine($"\n\n//Generated encoders for RPCs in this file (DO NOT TOUCH AND EDIT BY HANDS)");

            str.AppendLine($"public partial class {className}");
            str.AppendLine("{");
        }

        public void AppendClosure(StringBuilder str)
        {
            str.AppendLine("}");
            str.AppendLine("//Generation ends (DO NOT TOUCH AND EDIT BY HANDS) \n\n");
        }
    }
}

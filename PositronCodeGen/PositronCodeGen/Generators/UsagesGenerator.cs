using System.Text;

namespace PositronCodeGen.Generators
{
    internal class UsagesGenerator
    {
        public void GenerateUsages(StringBuilder str)
        {
            str.AppendLine("using UnityEngine;");
        }
    }
}

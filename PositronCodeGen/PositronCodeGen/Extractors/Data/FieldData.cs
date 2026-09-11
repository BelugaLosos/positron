using Microsoft.CodeAnalysis;

namespace PositronCodeGen.Extractors.Data
{
    internal struct FieldData
    {
        public string Name;
        public AttributeData Attribute;
        public bool HasDefaultInit;

        public FieldData(string name, AttributeData data, bool hasDefaultInit)
        {
            Name = name;
            Attribute = data;
            HasDefaultInit = hasDefaultInit;
        }
    }
}

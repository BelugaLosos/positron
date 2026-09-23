using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PositronCodeGen.Extractors.Data
{
    internal struct FieldData
    {
        public string Name;
        public AttributeData Attribute;
        public bool HasDefaultInit;
        public ITypeSymbol Type;
        public ImmutableArray<Location> Locations;

        public FieldData(string name, AttributeData data, bool hasDefaultInit, ITypeSymbol type, ImmutableArray<Location> locations)
        {
            Name = name;
            Attribute = data;
            HasDefaultInit = hasDefaultInit;
            Type = type;
            Locations = locations;
        }
    }
}

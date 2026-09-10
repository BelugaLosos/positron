using System;

namespace Positron.Client.NetValues.Attributes
{
    /// <summary>
    /// Mark for code analyser to generate implementations and annotation files
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NetworkedAttribute : Attribute
    {
        public NetValueAuthority Authority { get; set; }
        public bool IsPredictable { get; private set; }

        public NetworkedAttribute(NetValueAuthority authority, bool isPredicatble = false)
        {
            Authority = authority;
            IsPredictable = isPredicatble;
        }
    }
}
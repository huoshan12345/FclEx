using System.Collections.Generic;
using System.Linq;
using FclEx;
using FclEx.Extensions;

namespace System.Xml.Linq
{
    public class XElementEqualityComparer : IEqualityComparer<XElement>
    {
        public bool Equals(XElement? x, XElement? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            if (x.Value != y.Value) return false;

            var xProps = x.Attributes().OrderBy(m => m.Name);
            var yProps = y.Attributes().OrderBy(m => m.Name);
            return xProps.SequenceEqual(yProps, XAttributeEqualityComparer.Instance);
        }

        public int GetHashCode(XElement obj)
        {
            var hashCode = 2001076147;
            hashCode = hashCode * -1521134295 + obj.Name.GetHashCodeSafely();
            hashCode = hashCode * -1521134295 + obj.Value.GetHashCodeSafely();
            hashCode = hashCode * -1521134295 + obj.NodeType.GetHashCodeSafely();
            return hashCode;
        }

        public static XElementEqualityComparer Instance { get; } = new();
    }
}

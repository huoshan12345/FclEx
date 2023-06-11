using System.Collections.Generic;
using FclEx;
using FclEx.Extensions;

namespace System.Xml.Linq;

public class XAttributeEqualityComparer : IEqualityComparer<XAttribute>
{
    public bool Equals(XAttribute? x, XAttribute? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        return x.Name == y.Name
               && x.Value == y.Value
               && x.NodeType == y.NodeType;
    }

    public int GetHashCode(XAttribute obj)
    {
        var hashCode = 2001076147;
        hashCode = hashCode * -1521134295 + obj.Name.GetHashCodeSafely();
        hashCode = hashCode * -1521134295 + obj.Value.GetHashCodeSafely();
        hashCode = hashCode * -1521134295 + obj.NodeType.GetHashCodeSafely();
        return hashCode;
    }

    public static XAttributeEqualityComparer Instance { get; } = new();
}
using System.Collections.Generic;
using FclEx;

namespace System.Xml.Linq;

public class XAttributeEqualityComparer : IEqualityComparer<XAttribute>
{
    public bool Equals(XAttribute? x, XAttribute? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        return x.Name == y.Name
               && x.Value == y.Value
               && x.NodeType == y.NodeType;
    }

    public int GetHashCode(XAttribute obj)
    {
        var hash = new HashCode();
        hash.Add(obj.Name);
        hash.Add(obj.Value);
        hash.Add(obj.NodeType);
        return hash.ToHashCode();
    }

    public static XAttributeEqualityComparer Instance { get; } = new();
}
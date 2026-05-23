using System.Collections.Generic;
using System.Linq;
using FclEx;

namespace System.Xml.Linq;

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
        var hash = new HashCode();
        hash.Add(obj.Name);
        hash.Add(obj.Value);
        hash.Add(obj.NodeType);
        return hash.ToHashCode();
    }

    public static XElementEqualityComparer Instance { get; } = new();
}
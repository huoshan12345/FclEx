using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FclEx.Extensions;

public static class XElementExtensions
{
    public static string? AttributeValue(this XElement? e, string key, string? defaultValue = default)
    {
        return e?.Attribute(key)?.Value ?? defaultValue;
    }

    public static T AttributeValue<T>(this XElement? e, string key, Func<string?, T> func)
    {
        var value = e.AttributeValue(key);
        return func(value);
    }

    public static T Element<T>(this XElement? e, string key, Func<XElement?, T> func)
    {
        return func(e?.Element(key));
    }

    public static string? ElementValue(this XElement? e, string key, string? defaultValue = default)
    {
        return e?.Element(key)?.Value ?? defaultValue;
    }

    public static T ElementValue<T>(this XElement? e, string key, Func<string?, T> func)
    {
        var value = e.ElementValue(key);
        return func(value);
    }

    public static int ElementIntValue(this XElement? e, string key, int defaultValue = default)
    {
        return e.ElementValue(key, m => int.TryParse(m, out var result) ? result : defaultValue);
    }

    public static bool ElementBoolValue(this XElement? e, string key, bool defaultValue = default)
    {
        return e.ElementValue(key, m => bool.TryParse(m, out var result) ? result : defaultValue);
    }

    public static T ElementEnumValue<T>(this XElement? e, string key, bool ignoreCase = true, T defaultValue = default) where T : struct, Enum
    {
        return e.ElementValue(key, m => Enum.TryParse<T>(m, ignoreCase, out var r) ? r : defaultValue);
    }

    public static TimeSpan ElementTimeSpanValue(this XElement? e, string key, TimeSpan defaultValue = default)
    {
        return e.ElementValue(key, m => TimeSpan.TryParse(m, out var result) ? result : defaultValue);
    }

    public static TreeNode<XElement> ToTree(this XElement xml)
    {
        var root = new TreeNode<XElement>(xml);
        var map = new Dictionary<XElement, TreeNode<XElement>>()
        {
            { xml, root }
        };
        var queue = new Queue<XElement>();
        queue.Enqueue(xml);
        while (queue.Count != 0)
        {
            var item = queue.Dequeue();
            var node = map[item];
            item.Ancestors().ForEach(m =>
            {
                var child = node.AddChild(m);
                queue.Enqueue(m);
                map.Add(m, child);
            });
        }
        return root;
    }

    public static XElement RemoveComment(this XElement xml)
    {
        xml.DescendantNodes().OfType<XComment>().Remove();
        return xml;
    }

    public static T ToObject<T>(this XElement element)
    {
        return XmlHelper.Deserialize<T>(element);
    }
}
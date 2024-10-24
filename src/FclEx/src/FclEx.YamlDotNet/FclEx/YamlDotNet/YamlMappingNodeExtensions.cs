namespace FclEx.YamlDotNet;

public static class YamlMappingNodeExtensions
{
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public static T? Child<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        var (_, value) = node.Children.SingleOrDefault(m => m.Key.IsScalar(key));
        return value.CastTo<T>();
    }

    public static T RequiredChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.Child<T>(key) ?? throw new KeyNotFoundException($"Cannot find child whose type is '{typeof(T).SimpleName()}' by key '{key}'");
    }

    public static YamlMappingNode AddChild(this YamlMappingNode node, string key, string value, int? index = null)
    {
        var k = new YamlScalarNode(key);
        var v = new YamlScalarNode(value);
        if (index is { } i)
        {
            node.Children.Insert(i, k, v);
        }
        else
        {
            node.Children.Add(k, v);
        }
        return node;
    }

    public static YamlMappingNode RemoveChild(this YamlMappingNode node, string key)
    {
        var k = node.Children.SingleOrDefault(m => m.Key.IsScalar(key));
        if (k.Key != null)
        {
            node.Children.Remove(k.Key);
        }
        return node;
    }

    public static IEnumerable<(TKey Key, TValue Value)> Children<TKey, TValue>(this YamlMappingNode node)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        foreach (var (key, value) in node.Children)
        {
            yield return (key.CastTo<TKey>(), value.CastTo<TValue>());
        }
    }

    public static IEnumerable<(YamlScalarNode Key, TValue Value)> Children<TValue>(this YamlMappingNode node) where TValue : YamlNode
    {
        return node.Children<YamlScalarNode, TValue>();
    }

    public static IEnumerable<(YamlScalarNode Key, YamlNode Value)> Children(this YamlMappingNode node)
    {
        return node.Children<YamlNode>();
    }


    public static IEnumerable<T> Children<T>(this YamlMappingNode node, Func<KeyValuePair<YamlNode, YamlNode>, bool> filter) where T : YamlNode
    {
        return node.Children.Where(filter).Select(m => m.Value.CastTo<T>());
    }

    public static IEnumerable<T> Children<T>(this YamlMappingNode node, Func<YamlNode, YamlNode, bool> filter) where T : YamlNode
    {
        return node.Children<T>(m => filter(m.Key, m.Value));
    }

    public static IEnumerable<T> Children<T>(this YamlMappingNode node, Func<YamlNode, bool> keyFilter) where T : YamlNode
    {
        return node.Children<T>(m => keyFilter(m.Key));
    }

    public static IEnumerable<T> Children<T>(this YamlMappingNode node, IReadOnlyCollection<string> keys) where T : YamlNode
    {
        return node.Children<T>(m => keys.Any(x => m.Key.IsScalar(x)));
    }
}
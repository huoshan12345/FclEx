namespace FclEx.YamlDotNet;

public static class YamlMappingNodeExtensions
{
    public static T? Child<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        var (_, value) = node.Children.SingleOrDefault(m => m.Key.IsScalar(key));
        return value?.CastTo<T>();
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

    public static (YamlScalarNode Child, bool Changed) AddOrUpdateChild(this YamlMappingNode node, string key, string value,
        ScalarStyle valueStyle = ScalarStyle.Any, bool throwOnTypeMismatch = false)
    {
        var keyNode = new YamlScalarNode(key);

        if (node.Children.TryGetValue(keyNode, out var existingNode))
        {
            if (existingNode is YamlScalarNode scalarNode)
            {
                var valueChanged = scalarNode.Value != value;
                var styleChanged = scalarNode.Style != valueStyle;

                if (!valueChanged && !styleChanged)
                    return (scalarNode, false);

                scalarNode.Value = value;
                scalarNode.Style = valueStyle;
                return (scalarNode, true);
            }

            if (throwOnTypeMismatch)
            {
                throw new InvalidOperationException($"Key '{key}' already exists but is of type {existingNode.GetType().Name}, not expected {nameof(YamlScalarNode)}.");
            }

            node.Children.Remove(keyNode);
        }

        var newNode = new YamlScalarNode(value) { Style = valueStyle };
        node.Children.Add(keyNode, newNode);
        return (newNode, true);
    }

    public static (YamlScalarNode Child, bool Updated) AddOrUpdateChild(this YamlMappingNode node, string key, bool value)
    {
        return node.AddOrUpdateChild(key, value.ToLower(), ScalarStyle.Plain);
    }

    public static (T Child, bool Created) GetOrAddChild<T>(this YamlMappingNode node, string key, Func<T> factory)
        where T : YamlNode
    {
        var keyNode = new YamlScalarNode(key);
        if (node.Children.TryGetValue(keyNode, out var existingNode))
        {
            if (existingNode is T targetNode)
            {
                return (targetNode, false);
            }

            throw new InvalidOperationException($"Key '{key}' already exists but is of type {existingNode.GetType().Name}, not expected {typeof(T).Name}.");
        }

        var newNode = factory.Invoke();
        node.Add(keyNode, newNode);
        return (newNode, true);
    }

    public static (T Child, bool Created) GetOrAddChild<T>(this YamlMappingNode node, string key)
        where T : YamlNode, new()
    {
        return node.GetOrAddChild<T>(key, () => new T());
    }
}
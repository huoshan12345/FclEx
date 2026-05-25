namespace FclEx.YamlDotNet;

public static class YamlMappingNodeExtensions
{
    public static T? GetChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        return node.Children.TryGetValue(key, out var value)
            ? value as T
            : null;
    }

    public static T? GetChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.GetChild<T>(new YamlScalarNode(key));
    }

    public static T GetRequiredChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        if (node.Children.TryGetValue(key, out var value) == false)
            throw new KeyNotFoundException($"Cannot find child by key {FormatKey(key)}.");

        if (value is T child)
            return child;

        throw new InvalidOperationException($"Key {FormatKey(key)} exists but its child is of type {value.GetType().Name}, not expected {typeof(T).Name}.");
    }

    public static T GetRequiredChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.GetRequiredChild<T>(new YamlScalarNode(key));
    }

    public static YamlMappingNode AddScalarChild(this YamlMappingNode node, string key, string value, int? index = null)
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

    public static YamlMappingNode RemoveChild(this YamlMappingNode node, YamlNode key)
    {
        node.Children.Remove(key);
        return node;
    }

    public static YamlMappingNode RemoveChild(this YamlMappingNode node, string key)
    {
        return node.RemoveChild(new YamlScalarNode(key));
    }

    public static IEnumerable<KeyValuePair<TKey, TValue>> GetChildren<TKey, TValue>(this YamlMappingNode node, Func<YamlNode, YamlNode, bool>? filter = null)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        foreach (var (key, value) in node.Children)
        {
            if (filter == null || filter(key, value))
            {
                yield return KeyValuePair.Create(key.CastTo<TKey>(), value.CastTo<TValue>());
            }
        }
    }

    public static IEnumerable<KeyValuePair<YamlScalarNode, TValue>> GetChildren<TValue>(this YamlMappingNode node, Func<YamlNode, YamlNode, bool>? filter = null)
        where TValue : YamlNode
    {
        return node.GetChildren<YamlScalarNode, TValue>(filter);
    }

    public static IEnumerable<KeyValuePair<YamlScalarNode, YamlNode>> GetChildren(this YamlMappingNode node, Func<YamlNode, YamlNode, bool>? filter = null)
    {
        return node.GetChildren<YamlNode>(filter);
    }

    public static IEnumerable<KeyValuePair<YamlScalarNode, TValue>> GetChildren<TValue>(this YamlMappingNode node, IReadOnlyCollection<string> keys)
        where TValue : YamlNode
    {
        return node.GetChildren<YamlScalarNode, TValue>((k, v) => keys.Any(k.IsScalarValue));
    }

    public static (YamlScalarNode? Child, bool Changed) SetScalarChild(this YamlMappingNode node, string key, string value,
        ScalarStyle? valueStyle = null, YamlScalarChildConflictBehavior conflictBehavior = YamlScalarChildConflictBehavior.Replace)
    {
        var keyNode = new YamlScalarNode(key);

        if (node.Children.TryGetValue(keyNode, out var existingNode))
        {
            if (existingNode is YamlScalarNode scalarNode)
            {
                var valueChanged = scalarNode.Value != value;
                var styleChanged = valueStyle is not null && scalarNode.Style != valueStyle;

                if (!valueChanged && !styleChanged)
                    return (scalarNode, false);

                scalarNode.Value = value;
                scalarNode.Style = valueStyle ?? scalarNode.Style;
                return (scalarNode, true);
            }

            if (conflictBehavior == YamlScalarChildConflictBehavior.Throw)
            {
                throw new InvalidOperationException($"Key '{key}' already exists but is of type {existingNode.GetType().Name}, not expected {nameof(YamlScalarNode)}.");
            }

            if (conflictBehavior == YamlScalarChildConflictBehavior.Ignore)
                return (null, false);

            node.Children.Remove(keyNode);
        }

        var newNode = new YamlScalarNode(value) { Style = valueStyle ?? ScalarStyle.Any };
        node.Children.Add(keyNode, newNode);
        return (newNode, true);
    }

    public static (YamlScalarNode? Child, bool Changed) SetScalarChild(this YamlMappingNode node, string key, bool value)
    {
        return node.SetScalarChild(key, value.ToLower(), ScalarStyle.Plain);
    }

    public static (T Child, bool Added) GetOrAddChild<T>(this YamlMappingNode node, string key, Func<T> factory)
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

    public static (T Child, bool Added) GetOrAddChild<T>(this YamlMappingNode node, string key)
        where T : YamlNode, new()
    {
        return node.GetOrAddChild<T>(key, () => new T());
    }

    public static bool TryRemoveChildren<T>(this YamlMappingNode node, Func<YamlScalarNode, T, bool> match, out List<T> removedNodes)
    where T : YamlNode
    {
        removedNodes = [];
        var indexes = new List<int>();
        var index = 0;
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode k && value is T t && match(k, t))
            {
                removedNodes.Add(t);
                indexes.Add(index);
            }

            index++;
        }

        for (var i = indexes.Count - 1; i >= 0; i--)
        {
            node.Children.RemoveAt(indexes[i]);
        }

        return removedNodes.IsNotEmpty();
    }

    public static bool TryRemoveChildren<T>(this YamlMappingNode node, string key, out List<T> removedNodes) where T : YamlNode
    {
        return node.TryRemoveChildren((k, v) => k.Value == key, out removedNodes);
    }

    public static bool TryRemoveChildren(this YamlMappingNode node, string key)
    {
        return node.TryRemoveChildren<YamlNode>(key, out _);
    }

    public static (YamlNode? Key, T? Value, int Index) FindChild<T>(this YamlMappingNode node, Func<YamlNode, T, bool> match) where T : YamlNode
    {
        var index = 0;
        foreach (var (key, value) in node.Children)
        {
            if (value is T t && match(key, t))
                return (key, t, index);

            index++;
        }
        return (null, null, -1);
    }

    public static (YamlNode? Key, T? Value, int Index) FindChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        return node.FindChild<T>((k, _) => Equals(k, key));
    }

    public static (YamlScalarNode? Key, T? Value, int Index) FindChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        var (keyNode, value, index) = node.FindChild<T>((k, _) => k.IsScalarValue(key));
        return ((YamlScalarNode?)keyNode, value, index);
    }

    public static List<(YamlNode Key, T Value, int Index)> FindChildren<T>(this YamlMappingNode node, Func<YamlNode, T, bool> match) where T : YamlNode
    {
        var results = new List<(YamlNode Key, T Value, int Index)>();
        var index = 0;
        foreach (var (key, value) in node.Children)
        {
            if (value is T t && match(key, t))
                results.Add((key, t, index));

            index++;
        }
        return results;
    }

    public static List<(YamlScalarNode Key, T Value, int Index)> FindChildren<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.FindChildren<T>((k, _) => k.IsScalarValue(key))
            .Select(m => ((YamlScalarNode)m.Key, m.Value, m.Index))
            .ToList();
    }

    public static bool HasChild<T>(this YamlMappingNode node, Func<YamlNode, T, bool> match) where T : YamlNode
    {
        return node.FindChild(match).Key != null;
    }

    public static bool HasChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        return node.FindChild<T>(key).Key != null;
    }

    public static bool HasChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.HasChild<T>((k, _) => k.IsScalarValue(key));
    }

    public static bool HasChild(this YamlMappingNode node, string key, string value)
    {
        return node.HasChild<YamlScalarNode>((k, v) => k.IsScalarValue(key) && v.Value == value);
    }

    public static bool MoveChildByValueNode(this YamlMappingNode node, YamlNode valueNode, int destinationIndex)
    {
        var sourceIndex = node.Children.FindIndex(m => ReferenceEquals(m.Value, valueNode));
        if (sourceIndex < 0)
            throw new KeyNotFoundException("Child node not found in the YAML mapping node.");

        if (sourceIndex == destinationIndex)
            return false;

        node.Children.MoveAt(sourceIndex, destinationIndex);
        return true;
    }

    public static bool MoveChildByKeyNode(this YamlMappingNode node, YamlNode keyNode, int destinationIndex)
    {
        var sourceIndex = node.Children.FindIndex(m => ReferenceEquals(m.Key, keyNode));
        if (sourceIndex < 0)
            throw new KeyNotFoundException("Key node not found in the YAML mapping node.");

        if (sourceIndex == destinationIndex)
            return false;

        node.Children.MoveAt(sourceIndex, destinationIndex);
        return true;
    }

    public static bool MoveChildByKey(this YamlMappingNode node, string key, int destinationIndex)
    {
        var (_, _, sourceIndex) = node.FindChild<YamlNode>(key);
        if (sourceIndex < 0)
            throw new KeyNotFoundException($"Key '{key}' not found in the YAML mapping node.");

        if (sourceIndex == destinationIndex)
            return false;

        node.Children.MoveAt(sourceIndex, destinationIndex);
        return true;
    }

    private static string FormatKey(YamlNode key)
    {
        return key is YamlScalarNode scalar
            ? $"'{scalar.Value}'"
            : $"of type {key.GetType().Name}";
    }
}

namespace FclEx.YamlDotNet;

/// <summary>
/// Provides helpers for reading, updating, searching, and reordering YAML mapping node children.
/// </summary>
public static class YamlMappingNodeExtensions
{
    /// <summary>
    /// Gets a child value by YAML key when the value is assignable to the requested node type.
    /// </summary>
    /// <typeparam name="T">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The YAML key node to look up.</param>
    /// <returns>The child value, or <c>null</c> when the key is missing or the value has a different type.</returns>
    public static T? GetChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        return node.Children.TryGetValue(key, out var value)
            ? value as T
            : null;
    }

    /// <summary>
    /// Gets a child value by scalar key when the value is assignable to the requested node type.
    /// </summary>
    /// <typeparam name="T">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The scalar key value to look up.</param>
    /// <returns>The child value, or <c>null</c> when the key is missing or the value has a different type.</returns>
    public static T? GetChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.GetChild<T>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Gets a required child value by YAML key.
    /// </summary>
    /// <typeparam name="T">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The YAML key node to look up.</param>
    /// <returns>The child value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is missing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="T"/>.</exception>
    public static T GetRequiredChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        if (node.Children.TryGetValue(key, out var value) == false)
            throw new KeyNotFoundException($"Cannot find child by key {FormatKey(key)}.");

        if (value is T child)
            return child;

        throw new InvalidOperationException($"Key {FormatKey(key)} exists but its child is of type {value.GetType().Name}, not expected {typeof(T).Name}.");
    }

    /// <summary>
    /// Gets a required child value by scalar key.
    /// </summary>
    /// <typeparam name="T">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The scalar key value to look up.</param>
    /// <returns>The child value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is missing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="T"/>.</exception>
    public static T GetRequiredChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.GetRequiredChild<T>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Adds a scalar key/value pair to a mapping node.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add.</param>
    /// <param name="value">The scalar child value to add.</param>
    /// <param name="index">The optional zero-based insertion index. When <c>null</c>, the child is appended.</param>
    /// <returns>The same mapping node instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is outside the valid insertion range.</exception>
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

    /// <summary>
    /// Removes the child with the specified YAML key, if present.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The YAML key node to remove.</param>
    /// <returns>The same mapping node instance.</returns>
    public static YamlMappingNode RemoveChild(this YamlMappingNode node, YamlNode key)
    {
        node.Children.Remove(key);
        return node;
    }

    /// <summary>
    /// Removes the child with the specified scalar key, if present.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to remove.</param>
    /// <returns>The same mapping node instance.</returns>
    public static YamlMappingNode RemoveChild(this YamlMappingNode node, string key)
    {
        return node.RemoveChild(new YamlScalarNode(key));
    }

    /// <summary>
    /// Enumerates children whose key and value are assignable to the requested node types.
    /// </summary>
    /// <typeparam name="TKey">The expected key node type.</typeparam>
    /// <typeparam name="TValue">The expected value node type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="filter">An optional filter applied before key and value casts are performed.</param>
    /// <returns>The matching key/value pairs in document order.</returns>
    /// <remarks>When no filter is supplied, every child must be castable to the requested key and value types.</remarks>
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

    /// <summary>
    /// Enumerates children with scalar keys whose values are assignable to the requested node type.
    /// </summary>
    /// <typeparam name="TValue">The expected value node type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="filter">An optional filter applied before key and value casts are performed.</param>
    /// <returns>The matching scalar-key/value pairs in document order.</returns>
    public static IEnumerable<KeyValuePair<YamlScalarNode, TValue>> GetChildren<TValue>(this YamlMappingNode node, Func<YamlNode, YamlNode, bool>? filter = null)
        where TValue : YamlNode
    {
        return node.GetChildren<YamlScalarNode, TValue>(filter);
    }

    /// <summary>
    /// Enumerates children with scalar keys.
    /// </summary>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="filter">An optional filter applied before key casting is performed.</param>
    /// <returns>The matching scalar-key/value pairs in document order.</returns>
    public static IEnumerable<KeyValuePair<YamlScalarNode, YamlNode>> GetChildren(this YamlMappingNode node, Func<YamlNode, YamlNode, bool>? filter = null)
    {
        return node.GetChildren<YamlNode>(filter);
    }

    /// <summary>
    /// Enumerates children whose scalar keys are included in a key collection.
    /// </summary>
    /// <typeparam name="TValue">The expected value node type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="keys">The scalar key values to include. Duplicate key values do not duplicate output rows.</param>
    /// <returns>The matching scalar-key/value pairs in document order.</returns>
    public static IEnumerable<KeyValuePair<YamlScalarNode, TValue>> GetChildren<TValue>(this YamlMappingNode node, IReadOnlyCollection<string> keys)
        where TValue : YamlNode
    {
        return node.GetChildren<YamlScalarNode, TValue>((k, v) => keys.Any(k.IsScalarValue));
    }

    /// <summary>
    /// Adds or updates a scalar child value.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add or update.</param>
    /// <param name="value">The scalar value to write.</param>
    /// <param name="valueStyle">The optional scalar style. When omitted for an existing scalar, its current style is preserved.</param>
    /// <param name="conflictBehavior">Controls how an existing non-scalar child with the same key is handled.</param>
    /// <returns>
    /// The scalar child and whether the mapping changed. The child is <c>null</c> when a non-scalar conflict is ignored.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when a non-scalar child exists and <paramref name="conflictBehavior"/> is <see cref="YamlScalarChildConflictBehavior.Throw"/>.</exception>
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

    /// <summary>
    /// Adds or updates a boolean scalar child value using lower-case YAML boolean text and plain style.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add or update.</param>
    /// <param name="value">The boolean value to write.</param>
    /// <returns>The scalar child and whether the mapping changed.</returns>
    public static (YamlScalarNode? Child, bool Changed) SetScalarChild(this YamlMappingNode node, string key, bool value)
    {
        return node.SetScalarChild(key, value.ToLower(), ScalarStyle.Plain);
    }

    /// <summary>
    /// Gets an existing child or adds a new child produced by a factory.
    /// </summary>
    /// <typeparam name="T">The expected child node type.</typeparam>
    /// <param name="node">The mapping node to read or update.</param>
    /// <param name="key">The scalar key value to read or add.</param>
    /// <param name="factory">The factory used only when the key is missing.</param>
    /// <returns>The child and whether it was added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="T"/>.</exception>
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

    /// <summary>
    /// Gets an existing child or adds a new child by using the child node's parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The expected child node type.</typeparam>
    /// <param name="node">The mapping node to read or update.</param>
    /// <param name="key">The scalar key value to read or add.</param>
    /// <returns>The child and whether it was added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="T"/>.</exception>
    public static (T Child, bool Added) GetOrAddChild<T>(this YamlMappingNode node, string key)
        where T : YamlNode, new()
    {
        return node.GetOrAddChild<T>(key, () => new T());
    }

    /// <summary>
    /// Removes all scalar-keyed children whose values have the requested type and satisfy a predicate.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="match">The predicate applied to scalar keys and values assignable to <typeparamref name="T"/>.</param>
    /// <param name="removedNodes">The removed value nodes in their original order.</param>
    /// <returns><c>true</c> when at least one child is removed; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Removes all children with the specified scalar key whose values have the requested type.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to remove.</param>
    /// <param name="removedNodes">The removed value nodes in their original order.</param>
    /// <returns><c>true</c> when at least one child is removed; otherwise, <c>false</c>.</returns>
    public static bool TryRemoveChildren<T>(this YamlMappingNode node, string key, out List<T> removedNodes) where T : YamlNode
    {
        return node.TryRemoveChildren((k, v) => k.Value == key, out removedNodes);
    }

    /// <summary>
    /// Removes all children with the specified scalar key.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to remove.</param>
    /// <returns><c>true</c> when at least one child is removed; otherwise, <c>false</c>.</returns>
    public static bool TryRemoveChildren(this YamlMappingNode node, string key)
    {
        return node.TryRemoveChildren<YamlNode>(key, out _);
    }

    /// <summary>
    /// Finds the first child whose value has the requested type and satisfies a predicate.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="T"/>.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
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

    /// <summary>
    /// Finds the first child with the specified YAML key whose value has the requested type.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (YamlNode? Key, T? Value, int Index) FindChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        return node.FindChild<T>((k, _) => Equals(k, key));
    }

    /// <summary>
    /// Finds the first child with the specified scalar key whose value has the requested type.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <returns>The matched scalar key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (YamlScalarNode? Key, T? Value, int Index) FindChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        var (keyNode, value, index) = node.FindChild<T>((k, _) => k.IsScalarValue(key));
        return ((YamlScalarNode?)keyNode, value, index);
    }

    /// <summary>
    /// Finds all children whose values have the requested type and satisfy a predicate.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="T"/>.</param>
    /// <returns>The matched keys, values, and original zero-based indexes.</returns>
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

    /// <summary>
    /// Finds all children with the specified scalar key whose values have the requested type.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <returns>The matched scalar keys, values, and original zero-based indexes.</returns>
    public static List<(YamlScalarNode Key, T Value, int Index)> FindChildren<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.FindChildren<T>((k, _) => k.IsScalarValue(key))
            .Select(m => ((YamlScalarNode)m.Key, m.Value, m.Index))
            .ToList();
    }

    /// <summary>
    /// Determines whether any child value has the requested type and satisfies a predicate.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> when a matching child exists; otherwise, <c>false</c>.</returns>
    public static bool HasChild<T>(this YamlMappingNode node, Func<YamlNode, T, bool> match) where T : YamlNode
    {
        return node.FindChild(match).Key != null;
    }

    /// <summary>
    /// Determines whether a child with the specified YAML key has the requested value type.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match.</param>
    /// <returns><c>true</c> when a matching child exists; otherwise, <c>false</c>.</returns>
    public static bool HasChild<T>(this YamlMappingNode node, YamlNode key) where T : YamlNode
    {
        return node.FindChild<T>(key).Key != null;
    }

    /// <summary>
    /// Determines whether a child with the specified scalar key has the requested value type.
    /// </summary>
    /// <typeparam name="T">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <returns><c>true</c> when a matching child exists; otherwise, <c>false</c>.</returns>
    public static bool HasChild<T>(this YamlMappingNode node, string key) where T : YamlNode
    {
        return node.HasChild<T>((k, _) => k.IsScalarValue(key));
    }

    /// <summary>
    /// Determines whether a scalar child with the specified key and value exists.
    /// </summary>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <param name="value">The scalar child value to match.</param>
    /// <returns><c>true</c> when a matching scalar child exists; otherwise, <c>false</c>.</returns>
    public static bool HasChild(this YamlMappingNode node, string key, string value)
    {
        return node.HasChild<YamlScalarNode>((k, v) => k.IsScalarValue(key) && v.Value == value);
    }

    /// <summary>
    /// Moves a child identified by value-node reference to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="valueNode">The exact value node instance to move.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><c>true</c> when the child was moved; <c>false</c> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="valueNode"/> is not a value instance in the mapping.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
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

    /// <summary>
    /// Moves a child identified by key-node reference to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="keyNode">The exact key node instance to move.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><c>true</c> when the child was moved; <c>false</c> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="keyNode"/> is not a key instance in the mapping.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
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

    /// <summary>
    /// Moves a child identified by scalar key value to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="key">The scalar key value of the child to move.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><c>true</c> when the child was moved; <c>false</c> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no child with the specified scalar key exists.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
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

    /// <summary>
    /// Formats a key node for exception messages.
    /// </summary>
    private static string FormatKey(YamlNode key)
    {
        return key is YamlScalarNode scalar
            ? $"'{scalar.Value}'"
            : $"of type {key.GetType().Name}";
    }
}

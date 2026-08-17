namespace FclEx.YamlDotNet;

/// <summary>
/// Provides helpers for reading, updating, searching, and reordering YAML mapping node children.
/// </summary>
public static class YamlMappingNodeExtensions
{
    /// <summary>
    /// Gets a child value by YAML key when the value is assignable to the requested node type.
    /// </summary>
    /// <typeparam name="TValue">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The YAML key node to look up.</param>
    /// <returns>The child value, or <see langword="null"/> when the key is missing or the value has a different type.</returns>
    public static TValue? GetChild<TValue>(this YamlMappingNode node, YamlNode key) where TValue : YamlNode
    {
        return node.Children.TryGetValue(key, out var value)
            ? value as TValue
            : null;
    }

    /// <summary>
    /// Gets a child value by scalar key when the value is assignable to the requested node type.
    /// </summary>
    /// <typeparam name="TValue">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The scalar key value to look up.</param>
    /// <returns>The child value, or <see langword="null"/> when the key is missing or the value has a different type.</returns>
    public static TValue? GetChild<TValue>(this YamlMappingNode node, string key) where TValue : YamlNode
    {
        return node.GetChild<TValue>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Gets a required child value by YAML key.
    /// </summary>
    /// <typeparam name="TValue">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The YAML key node to look up.</param>
    /// <returns>The child value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is missing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="TValue"/>.</exception>
    public static TValue GetRequiredChild<TValue>(this YamlMappingNode node, YamlNode key) where TValue : YamlNode
    {
        if (node.Children.TryGetValue(key, out var value) == false)
            throw new KeyNotFoundException($"Cannot find child by key {FormatKey(key)}.");

        if (value is TValue child)
            return child;

        throw new InvalidOperationException($"Key {FormatKey(key)} exists but its child is of type {value.GetType().Name}, not expected {typeof(TValue).Name}.");
    }

    /// <summary>
    /// Gets a required child value by scalar key.
    /// </summary>
    /// <typeparam name="TValue">The expected child value type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="key">The scalar key value to look up.</param>
    /// <returns>The child value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is missing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="TValue"/>.</exception>
    public static TValue GetRequiredChild<TValue>(this YamlMappingNode node, string key) where TValue : YamlNode
    {
        return node.GetRequiredChild<TValue>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Adds a scalar key/value pair to a mapping node.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add.</param>
    /// <param name="value">The scalar child value to add.</param>
    /// <param name="index">The optional zero-based insertion index. When <see langword="null"/>, the child is appended.</param>
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
    /// <param name="filter">An optional filter applied after key and value type matching.</param>
    /// <returns>The matching key/value pairs in document order.</returns>
    /// <remarks>Children whose keys or values are not assignable to the requested node types are skipped.</remarks>
    public static IEnumerable<KeyValuePair<TKey, TValue>> GetChildren<TKey, TValue>(this YamlMappingNode node, Func<TKey, TValue, bool>? filter = null)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        foreach (var (key, value) in node.Children)
        {
            if (key is not TKey k || value is not TValue v)
                continue;

            if (filter?.Invoke(k, v) == false)
                continue;

            yield return KeyValuePair.Create(k, v);
        }
    }

    /// <summary>
    /// Enumerates children with scalar keys whose values are assignable to the requested node type.
    /// </summary>
    /// <typeparam name="TValue">The expected value node type.</typeparam>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="filter">An optional filter applied after key and value type matching.</param>
    /// <returns>The matching scalar-key/value pairs in document order.</returns>
    public static IEnumerable<KeyValuePair<YamlScalarNode, TValue>> GetChildren<TValue>(this YamlMappingNode node, Func<YamlScalarNode, TValue, bool>? filter = null)
        where TValue : YamlNode
    {
        return node.GetChildren<YamlScalarNode, TValue>(filter);
    }

    /// <summary>
    /// Enumerates children with scalar keys whose values are assignable to the requested node type.
    /// </summary>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="filter">An optional filter applied after key and value type matching.</param>
    /// <returns>The matching scalar-key/value pairs in document order.</returns>
    public static IEnumerable<KeyValuePair<YamlScalarNode, YamlNode>> GetChildren(this YamlMappingNode node, Func<YamlScalarNode, YamlNode, bool>? filter = null)
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
        return node.GetChildren<YamlScalarNode, TValue>((k, v) => keys.Any(k.IsScalarWithValue));
    }

    /// <summary>
    /// Enumerates children whose scalar keys are included in a key collection.
    /// </summary>
    /// <param name="node">The mapping node to read from.</param>
    /// <param name="keys">The scalar key values to include. Duplicate key values do not duplicate output rows.</param>
    /// <returns>The matching scalar-key/value pairs in document order.</returns>
    public static IEnumerable<KeyValuePair<YamlScalarNode, YamlNode>> GetChildren(this YamlMappingNode node, IReadOnlyCollection<string> keys)
    {
        return node.GetChildren<YamlNode>(keys);
    }

    /// <summary>
    /// Attempts to add or update a scalar child value.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add or update.</param>
    /// <param name="value">The scalar value to write.</param>
    /// <param name="valueStyle">The optional scalar style. When omitted for an existing scalar, its current style is preserved.</param>
    /// <param name="conflictBehavior">Controls how an existing non-scalar child with the same key is handled.</param>
    /// <returns>
    /// The scalar child and whether the mapping changed. The child is <see langword="null"/> when a non-scalar conflict is ignored; <c>Changed</c> only reports whether the mapping was modified.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when a non-scalar child exists and <paramref name="conflictBehavior"/> is <see cref="YamlScalarChildConflictBehavior.Throw"/>.</exception>
    public static (YamlScalarNode? Child, bool Changed) TrySetScalarChild(this YamlMappingNode node, string key, string value, ScalarStyle? valueStyle = null,
        YamlScalarChildConflictBehavior conflictBehavior = YamlScalarChildConflictBehavior.Ignore)
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
            {
                return (null, false);
            }

            node.Children.Remove(keyNode);
        }

        var newNode = new YamlScalarNode(value) { Style = valueStyle ?? ScalarStyle.Any };
        node.Children.Add(keyNode, newNode);
        return (newNode, true);
    }

    /// <summary>
    /// Attempts to add or update a boolean scalar child value using lower-case YAML boolean text and plain style.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add or update.</param>
    /// <param name="value">The boolean value to write.</param>
    /// <returns>The scalar child and whether the mapping changed.</returns>
    public static (YamlScalarNode? Child, bool Changed) TrySetScalarChild(this YamlMappingNode node, string key, bool value)
    {
        return node.TrySetScalarChild(key, value.ToLower(), ScalarStyle.Plain);
    }

    /// <summary>
    /// Adds or updates a scalar child value.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add or update.</param>
    /// <param name="value">The scalar value to write.</param>
    /// <param name="valueStyle">The optional scalar style. When omitted for an existing scalar, its current style is preserved.</param>
    /// <returns>
    /// The scalar child and whether the mapping changed.
    /// </returns>
    public static (YamlScalarNode Child, bool Changed) SetScalarChild(this YamlMappingNode node, string key, string value, ScalarStyle? valueStyle = null)
    {
        return node.TrySetScalarChild(key, value, valueStyle, YamlScalarChildConflictBehavior.Replace)!;
    }

    /// <summary>
    /// Adds or updates a boolean scalar child value using lower-case YAML boolean text and plain style.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to add or update.</param>
    /// <param name="value">The boolean value to write.</param>
    /// <returns>The scalar child and whether the mapping changed.</returns>
    public static (YamlScalarNode Child, bool Changed) SetScalarChild(this YamlMappingNode node, string key, bool value)
    {
        return node.SetScalarChild(key, value.ToLower(), ScalarStyle.Plain);
    }

    /// <summary>
    /// Gets an existing child or adds a new child produced by a factory.
    /// </summary>
    /// <typeparam name="TValue">The expected child node type.</typeparam>
    /// <param name="node">The mapping node to read or update.</param>
    /// <param name="key">The scalar key to read or add.</param>
    /// <param name="factory">The factory used only when the key is missing.</param>
    /// <returns>The child and whether it was added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="TValue"/>.</exception>
    public static (TValue Child, bool Added) GetOrAddChild<TValue>(this YamlMappingNode node, YamlNode key, Func<TValue> factory)
        where TValue : YamlNode
    {
        if (node.Children.TryGetValue(key, out var existingNode))
        {
            if (existingNode is TValue targetNode)
            {
                return (targetNode, false);
            }

            throw new InvalidOperationException($"Key '{key}' already exists but is of type {existingNode.GetType().Name}, not expected {typeof(TValue).Name}.");
        }

        var newNode = factory.Invoke();
        node.Add(key, newNode);
        return (newNode, true);
    }

    /// <summary>
    /// Gets an existing child or adds a new child produced by a factory.
    /// </summary>
    /// <typeparam name="TValue">The expected child node type.</typeparam>
    /// <param name="node">The mapping node to read or update.</param>
    /// <param name="key">The scalar key value to read or add.</param>
    /// <param name="factory">The factory used only when the key is missing.</param>
    /// <returns>The child and whether it was added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="TValue"/>.</exception>
    public static (TValue Child, bool Added) GetOrAddChild<TValue>(this YamlMappingNode node, string key, Func<TValue> factory)
        where TValue : YamlNode
    {
        return node.GetOrAddChild(new YamlScalarNode(key), factory);
    }

    /// <summary>
    /// Gets an existing child or adds a new child by using the child node's parameterless constructor.
    /// </summary>
    /// <typeparam name="TValue">The expected child node type.</typeparam>
    /// <param name="node">The mapping node to read or update.</param>
    /// <param name="key">The scalar key value to read or add.</param>
    /// <returns>The child and whether it was added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the key exists but the value is not assignable to <typeparamref name="TValue"/>.</exception>
    public static (TValue Child, bool Added) GetOrAddChild<TValue>(this YamlMappingNode node, string key)
        where TValue : YamlNode, new()
    {
        return node.GetOrAddChild(key, () => new TValue());
    }

    /// <summary>
    /// Removes all scalar-keyed children whose values have the requested type and satisfy a predicate.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="match">The predicate applied to scalar keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <param name="removedNodes">The removed value nodes in their original order.</param>
    /// <returns><see langword="true"/> when at least one child is removed; otherwise, <see langword="false"/>.</returns>
    public static bool TryRemoveChildren<TValue>(this YamlMappingNode node, Func<YamlScalarNode, TValue, bool> match, out List<TValue> removedNodes)
    where TValue : YamlNode
    {
        removedNodes = [];
        var indexes = new List<int>();
        var index = 0;
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode k && value is TValue t && match(k, t))
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
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to remove.</param>
    /// <param name="removedNodes">The removed value nodes in their original order.</param>
    /// <returns><see langword="true"/> when at least one child is removed; otherwise, <see langword="false"/>.</returns>
    public static bool TryRemoveChildren<TValue>(this YamlMappingNode node, string key, out List<TValue> removedNodes) where TValue : YamlNode
    {
        return node.TryRemoveChildren((k, v) => k.Value == key, out removedNodes);
    }

    /// <summary>
    /// Removes all children with the specified scalar key.
    /// </summary>
    /// <param name="node">The mapping node to update.</param>
    /// <param name="key">The scalar key value to remove.</param>
    /// <returns><see langword="true"/> when at least one child is removed; otherwise, <see langword="false"/>.</returns>
    public static bool TryRemoveChildren(this YamlMappingNode node, string key)
    {
        return node.TryRemoveChildren<YamlNode>(key, out _);
    }

    /// <summary>
    /// Finds the first child whose value has the requested type and satisfies a predicate.
    /// </summary>
    /// <typeparam name="TKey">The key node type to match.</typeparam>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (TKey? Key, TValue? Value, int Index) FindChild<TKey, TValue>(this YamlMappingNode node, Func<TKey, TValue, bool> match)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        var index = 0;
        foreach (var (key, value) in node.Children)
        {
            if (key is TKey k && value is TValue t && match(k, t))
                return (k, t, index);

            index++;
        }
        return (null, null, -1);
    }

    /// <summary>
    /// Finds the first child with the specified YAML key whose value has the requested type.
    /// </summary>
    /// <typeparam name="TKey">The key node type to match.</typeparam>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match by YAML node equality.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (TKey? Key, TValue? Value, int Index) FindChild<TKey, TValue>(this YamlMappingNode node, TKey key)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        return node.FindChild<TKey, TValue>((k, _) => Equals(k, key));
    }

    /// <summary>
    /// Finds the first child with the specified YAML key whose value has the requested type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match by YAML node equality.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (YamlNode? Key, TValue? Value, int Index) FindChild<TValue>(this YamlMappingNode node, YamlNode key)
        where TValue : YamlNode
    {
        return node.FindChild<YamlNode, TValue>(key);
    }

    /// <summary>
    /// Finds the first child whose value has the requested type and satisfies a predicate.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (YamlScalarNode? Key, TValue? Value, int Index) FindChild<TValue>(this YamlMappingNode node, Func<YamlScalarNode, TValue, bool> match)
         where TValue : YamlNode
    {
        return node.FindChild<YamlScalarNode, TValue>(match);
    }

    /// <summary>
    /// Finds the first child with the specified YAML key whose value has the requested type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match.</param>
    /// <returns>The matched key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (YamlScalarNode? Key, TValue? Value, int Index) FindChild<TValue>(this YamlMappingNode node, YamlScalarNode key)
        where TValue : YamlNode
    {
        return node.FindChild<TValue>((k, _) => Equals(k, key));
    }

    /// <summary>
    /// Finds the first child with the specified scalar key whose value has the requested type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <returns>The matched scalar key, value, and zero-based index, or <c>(null, null, -1)</c> when no child matches.</returns>
    public static (YamlScalarNode? Key, TValue? Value, int Index) FindChild<TValue>(this YamlMappingNode node, string key) where TValue : YamlNode
    {
        return node.FindChild<TValue>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Finds all children whose values have the requested type and satisfy a predicate.
    /// </summary>
    /// <typeparam name="TKey">The key node type to match.</typeparam>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <returns>The matched keys, values, and original zero-based indexes.</returns>
    public static List<(TKey Key, TValue Value, int Index)> FindChildren<TKey, TValue>(this YamlMappingNode node, Func<TKey, TValue, bool> match)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        var results = new List<(TKey Key, TValue Value, int Index)>();
        var index = 0;
        foreach (var (key, value) in node.Children)
        {
            if (key is TKey k && value is TValue t && match(k, t))
                results.Add((k, t, index));

            index++;
        }
        return results;
    }

    /// <summary>
    /// Finds all children whose values have the requested type and satisfy a predicate.
    /// </summary>
    /// <typeparam name="TKey">The key node type to match.</typeparam>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The key node to match by YAML node equality.</param>
    /// <returns>The matched keys, values, and original zero-based indexes.</returns>
    public static List<(TKey Key, TValue Value, int Index)> FindChildren<TKey, TValue>(this YamlMappingNode node, TKey key)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        return node.FindChildren<TKey, TValue>((k, _) => Equals(k, key));
    }

    /// <summary>
    /// Finds all children with the specified YAML key whose values have the requested type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match by YAML node equality.</param>
    /// <returns>The matched keys, values, and original zero-based indexes.</returns>
    public static List<(YamlNode Key, TValue Value, int Index)> FindChildren<TValue>(this YamlMappingNode node, YamlNode key)
        where TValue : YamlNode
    {
        return node.FindChildren<YamlNode, TValue>(key);
    }

    /// <summary>
    /// Finds all children whose values have the requested type and satisfy a predicate.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <returns>The matched keys, values, and original zero-based indexes.</returns>
    public static List<(YamlScalarNode Key, TValue Value, int Index)> FindChildren<TValue>(this YamlMappingNode node, Func<YamlScalarNode, TValue, bool> match)
        where TValue : YamlNode
    {
        return node.FindChildren<YamlScalarNode, TValue>(match);
    }

    /// <summary>
    /// Finds all children with the specified scalar key whose values have the requested type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key node to match.</param>
    /// <returns>The matched scalar keys, values, and original zero-based indexes.</returns>
    public static List<(YamlScalarNode Key, TValue Value, int Index)> FindChildren<TValue>(this YamlMappingNode node, YamlScalarNode key) where TValue : YamlNode
    {
        return node.FindChildren<YamlScalarNode, TValue>((k, _) => Equals(k, key));
    }

    /// <summary>
    /// Finds all children with the specified scalar key whose values have the requested type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <returns>The matched scalar keys, values, and original zero-based indexes.</returns>
    public static List<(YamlScalarNode Key, TValue Value, int Index)> FindChildren<TValue>(this YamlMappingNode node, string key) where TValue : YamlNode
    {
        return node.FindChildren<TValue>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Determines whether any child value has the requested type and satisfies a predicate.
    /// </summary>
    /// <typeparam name="TKey">The key node type to match.</typeparam>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <returns><see langword="true"/> when a matching child exists; otherwise, <see langword="false"/>.</returns>
    public static bool HasChild<TKey, TValue>(this YamlMappingNode node, Func<TKey, TValue, bool> match)
        where TKey : YamlNode
        where TValue : YamlNode
    {
        return node.FindChild(match).Key != null;
    }

    /// <summary>
    /// Determines whether any child value has the requested type and satisfies a predicate.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="match">The predicate applied to keys and values assignable to <typeparamref name="TValue"/>.</param>
    /// <returns><see langword="true"/> when a matching child exists; otherwise, <see langword="false"/>.</returns>
    public static bool HasChild<TValue>(this YamlMappingNode node, Func<YamlScalarNode, TValue, bool> match)
        where TValue : YamlNode
    {
        return node.HasChild<YamlScalarNode, TValue>(match);
    }

    /// <summary>
    /// Determines whether a child with the specified YAML key has the requested value type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The YAML key node to match by YAML node equality.</param>
    /// <returns><see langword="true"/> when a matching child exists; otherwise, <see langword="false"/>.</returns>
    public static bool HasChild<TValue>(this YamlMappingNode node, YamlNode key) where TValue : YamlNode
    {
        return node.HasChild<YamlNode, TValue>((k, v) => Equals(k, key));
    }

    /// <summary>
    /// Determines whether a child with the specified scalar key has the requested value type.
    /// </summary>
    /// <typeparam name="TValue">The value node type to match.</typeparam>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <returns><see langword="true"/> when a matching child exists; otherwise, <see langword="false"/>.</returns>
    public static bool HasChild<TValue>(this YamlMappingNode node, string key) where TValue : YamlNode
    {
        return node.HasChild<TValue>(new YamlScalarNode(key));
    }

    /// <summary>
    /// Determines whether a scalar child with the specified key and value exists.
    /// </summary>
    /// <param name="node">The mapping node to search.</param>
    /// <param name="key">The scalar key value to match.</param>
    /// <param name="value">The scalar child value to match.</param>
    /// <returns><see langword="true"/> when a matching scalar child exists; otherwise, <see langword="false"/>.</returns>
    public static bool HasChild(this YamlMappingNode node, string key, string value)
    {
        return node.HasChild<YamlScalarNode, YamlScalarNode>((k, v) => k.IsScalarWithValue(key) && v.Value == value);
    }

    /// <summary>
    /// Moves the first child whose value is equal to the specified value node to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="valueNode">The value node to match by YAML node equality.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><see langword="true"/> when the child was moved; <see langword="false"/> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no equal value node exists in the mapping.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
    public static bool MoveChildByValue(this YamlMappingNode node, YamlNode valueNode, int destinationIndex)
    {
        var sourceIndex = node.Children.FindIndex(m => Equals(m.Value, valueNode));
        if (sourceIndex < 0)
            throw new KeyNotFoundException("Child node not found in the YAML mapping node.");

        if (sourceIndex == destinationIndex)
            return false;

        node.Children.MoveAt(sourceIndex, destinationIndex);
        return true;
    }

    /// <summary>
    /// Moves the child whose value is the specified value node instance to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="valueNode">The exact value node instance to move.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><see langword="true"/> when the child was moved; <see langword="false"/> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="valueNode"/> is not a value instance in the mapping.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
    public static bool MoveChildByValueReference(this YamlMappingNode node, YamlNode valueNode, int destinationIndex)
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
    /// Moves the first child whose key is equal to the specified key node to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="keyNode">The key node to match by YAML node equality.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><see langword="true"/> when the child was moved; <see langword="false"/> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no equal key node exists in the mapping.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
    public static bool MoveChildByKey(this YamlMappingNode node, YamlNode keyNode, int destinationIndex)
    {
        var sourceIndex = node.Children.FindIndex(m => Equals(m.Key, keyNode));
        if (sourceIndex < 0)
            throw new KeyNotFoundException("Key node not found in the YAML mapping node.");

        if (sourceIndex == destinationIndex)
            return false;

        node.Children.MoveAt(sourceIndex, destinationIndex);
        return true;
    }

    /// <summary>
    /// Moves the child whose key is the specified key node instance to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="keyNode">The exact key node instance to move.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><see langword="true"/> when the child was moved; <see langword="false"/> when it was already at the destination index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="keyNode"/> is not a key instance in the mapping.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destinationIndex"/> is outside the mapping bounds.</exception>
    public static bool MoveChildByKeyReference(this YamlMappingNode node, YamlNode keyNode, int destinationIndex)
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
    /// Moves the first child with the specified scalar key value to a destination index.
    /// </summary>
    /// <param name="node">The mapping node to reorder.</param>
    /// <param name="key">The scalar key value of the child to move.</param>
    /// <param name="destinationIndex">The zero-based destination index.</param>
    /// <returns><see langword="true"/> when the child was moved; <see langword="false"/> when it was already at the destination index.</returns>
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

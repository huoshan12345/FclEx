namespace FclEx.YamlDotNet;

/// <summary>
/// Provides helpers for searching and removing children in YAML sequence nodes.
/// </summary>
public static class YamlSequenceNodeExtensions
{
    /// <summary>
    /// Finds the first child of the specified type that satisfies a predicate.
    /// </summary>
    /// <typeparam name="T">The child node type to match.</typeparam>
    /// <param name="node">The sequence node to search.</param>
    /// <param name="match">The predicate applied to children assignable to <typeparamref name="T"/>.</param>
    /// <returns>The matched child and its zero-based index, or <c>(null, -1)</c> when no child matches.</returns>
    public static (T? Child, int Index) FindChild<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        var index = 0;
        foreach (var child in node.Children)
        {
            if (child is T t && match(t))
                return (t, index);

            index++;
        }
        return (null, -1);
    }

    /// <summary>
    /// Finds all children of the specified type that satisfy a predicate.
    /// </summary>
    /// <typeparam name="T">The child node type to match.</typeparam>
    /// <param name="node">The sequence node to search.</param>
    /// <param name="match">The predicate applied to children assignable to <typeparamref name="T"/>.</param>
    /// <returns>A list of matched children with their original zero-based indexes.</returns>
    public static List<(T Child, int Index)> FindChildren<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        var index = 0;
        var list = new List<(T Child, int Index)>();
        foreach (var child in node.Children)
        {
            if (child is T t && match(t))
                list.Add((t, index));

            index++;
        }
        return list;
    }

    /// <summary>
    /// Determines whether any child of the specified type satisfies a predicate.
    /// </summary>
    /// <typeparam name="T">The child node type to match.</typeparam>
    /// <param name="node">The sequence node to search.</param>
    /// <param name="match">The predicate applied to children assignable to <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> when a matching child exists; otherwise, <c>false</c>.</returns>
    public static bool HasChild<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        return node.FindChild(match).Child != null;
    }

    /// <summary>
    /// Removes all children of the specified type that satisfy a predicate.
    /// </summary>
    /// <typeparam name="T">The child node type to match.</typeparam>
    /// <param name="node">The sequence node to update.</param>
    /// <param name="match">The predicate applied to children assignable to <typeparamref name="T"/>.</param>
    /// <param name="removedNodes">The removed nodes in their original order.</param>
    /// <returns><see langword="true"/> when at least one child is removed; otherwise, <c>false</c>.</returns>
    public static bool TryRemoveChildren<T>(this YamlSequenceNode node, Predicate<T> match, out List<T> removedNodes) where T : YamlNode
    {
        removedNodes = [];
        for (var i = node.Children.Count - 1; i >= 0; i--)
        {
            var child = node.Children[i];
            if (child is T t && match(t))
            {
                removedNodes.Add(t);
                node.Children.RemoveAt(i);
            }
        }

        removedNodes.Reverse();
        return removedNodes.IsNotEmpty();
    }

    /// <summary>
    /// Removes all children of the specified type that satisfy a predicate.
    /// </summary>
    /// <typeparam name="T">The child node type to match.</typeparam>
    /// <param name="node">The sequence node to update.</param>
    /// <param name="match">The predicate applied to children assignable to <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> when at least one child is removed; otherwise, <c>false</c>.</returns>
    public static bool TryRemoveChildren<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        return node.TryRemoveChildren(match, out _);
    }
}

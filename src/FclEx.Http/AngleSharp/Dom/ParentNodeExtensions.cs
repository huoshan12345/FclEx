namespace AngleSharp.Dom;

/// <summary>
/// Extensions for mutating AngleSharp parent nodes.
/// </summary>
public static class ParentNodeExtensions
{
    /// <summary>
    /// Removes all descendant <c>script</c> and <c>style</c> elements from a node and returns the same node instance.
    /// A <see langword="null"/> input returns <see langword="null"/>.
    /// </summary>
    [return: NotNullIfNotNull(nameof(node))]
    public static T? RemoveJsCss<T>(this T? node) where T : IParentNode
    {
        if (node == null)
            return default;

        foreach (var childNode in node.QuerySelectorAll("script, style"))
        {
            childNode.Remove();
        }
        return node;
    }
}

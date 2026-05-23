namespace AngleSharp.Dom;

public static class ParentNodeExtensions
{
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

namespace AngleSharp.Dom;

/// <summary>
/// Extensions for extracting direct text content from AngleSharp nodes.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Returns text from direct child text nodes only.
    /// Text inside descendant elements is intentionally excluded.
    /// </summary>
    public static string OwnText(this INode element)
    {
        using var disposable = StringBuilder.GetCached();
        var builder = disposable.Value;

        foreach (var node in element.ChildNodes.OfType<IText>())
        {
            builder.Append(node.Data);
        }
        return builder.ToString();
    }
}

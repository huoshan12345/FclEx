namespace FclEx.Extensions;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Retrieves the child <see cref="JsonNode"/> at the specified <paramref name="key"/> if it exists
    /// and is of type <typeparamref name="TNode"/>.  
    /// If not, creates a new instance of <typeparamref name="TNode"/> using the provided <paramref name="creator"/>,
    /// assigns it to the <paramref name="key"/> in the parent <paramref name="node"/>, and returns it.
    /// </summary>
    /// <typeparam name="TNode">The expected type of the child node.</typeparam>
    /// <param name="node">The parent <see cref="JsonNode"/> to search in.</param>
    /// <param name="key">The property name to retrieve or assign.</param>
    /// <param name="creator">A factory function used to create a new <typeparamref name="TNode"/> if none exists.</param>
    /// <returns>
    /// The existing <typeparamref name="TNode"/> at the given <paramref name="key"/>, 
    /// or the newly created one if none was found.
    /// </returns>
    public static TNode GetOrAdd<TNode>(this JsonNode node, string key, Func<TNode> creator) where TNode : JsonNode
    {
        if (node[key] is TNode existing)
            return existing;

        var newNode = creator();
        node[key] = newNode;
        return newNode;
    }


    /// <summary>
    /// Converts a <see cref="JsonNode"/> to its string representation.
    /// </summary>
    /// <param name="node">The JSON node to convert.</param>
    /// <param name="options">Optional serializer options. If not provided, default options from <see cref="JsonHelper.GetOptions"/> are used.</param>
    /// <returns>
    /// A string representation of the node:<br/>
    /// If <paramref name="node"/> is <c>null</c>, returns <c>null</c>.<br/>
    /// If <paramref name="node"/> is a <see cref="JsonValue"/> containing a string, returns that string directly.<br/>
    /// Otherwise, serializes the node to a JSON string using the provided or default options.
    /// </returns>
    public static string? ToValueString(this JsonNode? node, JsonSerializerOptions? options = null)
    {
        options ??= JsonHelper.GetOptions();
        return node switch
        {
            null => null,
            JsonValue value when value.TryGetValue<string>(out var str) => str,
            _ => node.ToJsonString(options),
        };
    }
}
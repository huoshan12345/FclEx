namespace FclEx.Extensions;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Retrieves the child <see cref="JsonNode"/> at the specified <paramref name="key"/> if it exists
    /// and is of type <typeparamref name="TNode"/>. If the property is missing or contains JSON null, creates a new
    /// instance using <paramref name="creator"/>, assigns it to the parent, and returns it.
    /// </summary>
    /// <typeparam name="TNode">The expected type of the child node.</typeparam>
    /// <param name="node">The parent <see cref="JsonNode"/> to search in.</param>
    /// <param name="key">The property name to retrieve or assign.</param>
    /// <param name="creator">A factory function used to create a new <typeparamref name="TNode"/> if none exists.</param>
    /// <returns>
    /// The existing <typeparamref name="TNode"/> at the given <paramref name="key"/>, 
    /// or the newly created one if none was found.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The property exists and contains a node whose type is not <typeparamref name="TNode"/>.
    /// </exception>
    public static TNode GetOrAdd<TNode>(this JsonNode node, string key, Func<TNode> creator) where TNode : JsonNode
    {
        var current = node[key];
        if (current is TNode existing)
            return existing;

        if (current is not null)
        {
            throw new InvalidOperationException(
                $"The JSON property '{key}' contains a node of type '{current.GetType().Name}', not '{typeof(TNode).Name}'.");
        }

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
    /// If <paramref name="node"/> is <see langword="null"/>, returns <see langword="null"/>.<br/>
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

    public static JsonElement? ToJsonElement(this JsonNode node, JsonSerializerOptions? options = null)
    {
        return node.Deserialize<JsonElement>(options);
    }

    public static JsonElement? ToJsonElement(this JsonNode node, JsonOptions options)
    {
        return node.ToJsonElement(JsonHelper.GetOptions(options));
    }

    public static string ToJsonString(this JsonNode node, JsonOptions options)
    {
        return node.ToJsonString(JsonHelper.GetOptions(options));
    }

    public static T? ToObject<T>(this JsonNode node, JsonSerializerOptions? options = null)
    {
        return node.Deserialize<T>(options ?? JsonHelper.GetOptions());
    }

    public static T? ToObject<T>(this JsonNode node, JsonOptions options)
    {
        return node.ToObject<T>(JsonHelper.GetOptions(options));
    }

    /// <summary>
    /// Selects nodes matching the specified JSONPath.
    /// </summary>
    /// <param name="root">The JSON value against which the path is evaluated.</param>
    /// <param name="jsonPath">
    /// The JSONPath expression. When <see langword="null"/>, the root node is returned.
    /// </param>
    /// <returns>The nodes matched by the path.</returns>
    public static IEnumerable<JsonNode?> SelectNodes(this JsonNode? root, string? jsonPath)
    {
        if (jsonPath is null)
            return [root];

        var path = Json.Path.JsonPath.Parse(jsonPath);
        return path.Evaluate(root).Matches.Select(m => m.Value);
    }

    extension(JsonNode)
    {
        public static JsonNode? From<T>(T obj, JsonSerializerOptions? options = null)
        {
            options ??= JsonHelper.GetOptions();
            return JsonSerializer.SerializeToNode(obj, options);
        }
    }
}

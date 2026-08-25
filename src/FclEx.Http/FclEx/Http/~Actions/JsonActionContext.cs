namespace FclEx.Http;

/// <summary>
/// Contains a parsed JSON response and the tokens selected for an action.
/// </summary>
/// <remarks>Selected tokens are cloned during construction, so they remain readable after the parser document is disposed.</remarks>
public readonly struct JsonActionContext
{
    /// <summary>
    /// Initializes a JSON action context.
    /// </summary>
    /// <param name="response">The source HTTP response.</param>
    /// <param name="json">The JSON text to parse.</param>
    /// <param name="jsonPath">The optional JSON path. When <see langword="null"/>, the root token is selected.</param>
    /// <remarks>Malformed JSON may throw during construction.</remarks>
    public JsonActionContext(HttpResponse response, string json, string? jsonPath)
    {
        Response = response;
        Json = json;
        JsonPath = jsonPath;
        var root = JsonNode.Parse(json);
        Token = root;
        ResultTokens = root.SelectNodes(jsonPath);
    }

    /// <summary>
    /// Gets the source HTTP response.
    /// </summary>
    public HttpResponse Response { get; }

    /// <summary>
    /// Gets the JSON path used to select result tokens.
    /// </summary>
    public string? JsonPath { get; }

    /// <summary>
    /// Gets the original JSON text.
    /// </summary>
    public string Json { get; }

    /// <summary>
    /// Gets the root JSON token.
    /// </summary>
    public JsonNode? Token { get; }

    /// <summary>
    /// Gets the selected result tokens.
    /// </summary>
    public IEnumerable<JsonNode?> ResultTokens { get; }

    /// <summary>
    /// Gets the first selected result token, or <see langword="null"/> when no token matched.
    /// </summary>
    public JsonNode? ResultToken => ResultTokens.FirstOrDefault();
}

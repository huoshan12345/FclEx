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
        using var jsonDocument = JsonDocument.Parse(json);
        Token = jsonDocument.RootElement.Clone();
        ResultTokens = jsonPath == null
            ? [Token]
            : jsonDocument.RootElement.SelectElements(jsonPath, false)
                .NotNull()
                .Select(token => token.Clone())
                .AsIReadOnlyList();
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
    public JsonElement Token { get; }

    /// <summary>
    /// Gets the selected result tokens.
    /// </summary>
    public IReadOnlyList<JsonElement> ResultTokens { get; }

    /// <summary>
    /// Gets the first selected result token, or <see langword="null"/> when no token matched.
    /// </summary>
    public JsonElement? ResultToken => TryGetResultToken(out var token) ? token : null;

    /// <summary>
    /// Gets the first selected result token, if any.
    /// </summary>
    /// <param name="token">The first selected result token, or the default value when no token matched.</param>
    /// <returns><see langword="true"/> when a result token exists; otherwise, <see langword="false"/>.</returns>
    /// <remarks>Use this method instead of <c>FirstOrDefault</c>, because <see cref="JsonElement"/> is a struct and its default value does not indicate whether a token matched.</remarks>
    public bool TryGetResultToken(out JsonElement token)
    {
        if (ResultTokens.Count > 0)
        {
            token = ResultTokens[0];
            return true;
        }

        token = default;
        return false;
    }
}

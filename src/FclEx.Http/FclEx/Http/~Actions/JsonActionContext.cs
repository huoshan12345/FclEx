namespace FclEx.Http;

/// <summary>
/// Contains a parsed JSON response and the tokens selected for an action.
/// </summary>
/// <remarks>Dispose the context when using it outside the default JSON action pipeline.</remarks>
public readonly struct JsonActionContext : IDisposable
{
    private readonly JsonDocument _jsonDocument;

    /// <summary>
    /// Initializes a JSON action context.
    /// </summary>
    /// <param name="response">The source HTTP response.</param>
    /// <param name="json">The JSON text to parse.</param>
    /// <param name="path">The optional JSON path. When <see langword="null"/>, the root token is selected.</param>
    /// <remarks>Malformed JSON may throw during construction.</remarks>
    public JsonActionContext(HttpResponse response, string json, string? path)
    {
        Response = response;
        Json = json;
        Path = path;
        _jsonDocument = JsonDocument.Parse(json);
        Token = _jsonDocument.RootElement;
        ResultTokens = path == null
            ? [Token]
            : Token.SelectElements(path, false).NotNull().AsIReadOnlyList();
    }

    /// <summary>
    /// Gets the source HTTP response.
    /// </summary>
    public HttpResponse Response { get; }

    /// <summary>
    /// Gets the JSON path used to select result tokens.
    /// </summary>
    public string? Path { get; }

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
    public JsonElement? ResultToken => ResultTokens.FirstOrDefault();

    /// <summary>
    /// Disposes the underlying JSON document.
    /// </summary>
    public void Dispose()
    {
        _jsonDocument.Dispose();
    }
}

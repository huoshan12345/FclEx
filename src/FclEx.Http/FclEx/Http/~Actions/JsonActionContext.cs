namespace FclEx.Http;

public readonly struct JsonActionContext : IDisposable
{
    private readonly JsonDocument _jsonDocument;

    public JsonActionContext(HttpResponse response, string json, string? path)
    {
        Response = response;
        Json = json;
        Path = path;
        _jsonDocument = JsonDocument.Parse(json);
        Token = _jsonDocument.RootElement;
        ResultTokens = path == null
            ? [Token]
            : Token.SelectElements(path, false).NotNull();
    }

    public HttpResponse Response { get; }
    public string? Path { get; }
    public string Json { get; }
    public JsonElement Token { get; }
    public IEnumerable<JsonElement> ResultTokens { get; }
    public JsonElement? ResultToken => ResultTokens.FirstOrDefault();

    public void Dispose()
    {
        _jsonDocument.Dispose();
    }
}
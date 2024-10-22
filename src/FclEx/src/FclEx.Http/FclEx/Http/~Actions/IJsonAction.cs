using System.Text.Json;

namespace FclEx.Http;

public interface IJsonAction<T> : IHttpResponseHandler<T>
{
    string? JsonResultPath { get; }

    OperateResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse res)
    {
        var (successful, str, ex, _) = GetJson(res);
        if (!successful)
            return ex!;

        var context = new JsonActionContext(res, str!, JsonResultPath);

        return IsFailed(context) 
            ? HandleFailed(context) 
            : GetResult(context);
    }

    OperateResult<string> GetJson(HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleJson()
            ? Operate.CreateSuccess(response.ResponseString)
            : Operate.CreateError<string>("The res string is not a valid json: " + str.Truncate(256));
    }

    bool IsFailed(JsonActionContext context) => !context.ResultTokens.Any();

    OperateResult<T> HandleFailed(JsonActionContext context)
    {
        const string msg = "The result object does not exist in json";
        var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
        error = error + ": " + context.Json.Truncate(256);
        return error;
    }

    OperateResult<T> GetResult(JsonActionContext context)
    {
        return context.ResultToken is { } token
            ? token.Deserialize<T>()!
            : nameof(context.ResultToken) + " is null";
    }
}

public interface IJsonAction : IJsonAction<Unit>
{
    OperateResult IJsonAction<Unit>.GetResult(JsonActionContext context) => Operate.Success;
}

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
            ? Token.Yield()
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
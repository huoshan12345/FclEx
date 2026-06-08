namespace FclEx.Http.Actions;

internal static class HttpActionTestFixtures
{
    public static readonly TimeSpan Elapsed = TimeSpan.FromMilliseconds(123);

    public static HttpResponse CreateResponse(
        string responseString = "",
        HttpStatusCode statusCode = HttpStatusCode.OK,
        TimeSpan? elapsed = null,
        Exception? exception = null,
        string requestUri = "https://example.com/source")
    {
        var request = HttpRequest.Get(requestUri);
        var response = exception is null 
            ? new HttpResponse(request) 
            : HttpResponse.FromError(request, exception);
        response.ResponseString = responseString;
        response.StatusCode = statusCode;
        response.Elapsed = elapsed ?? TimeSpan.Zero;
        return response;
    }

    public static string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".html");
        File.WriteAllText(path, content);
        return path;
    }
}

internal sealed class StubHttpService : IHttpService
{
    private readonly Func<HttpRequest, CancellationToken, Task<HttpResponse>> _sendAsync;

    public StubHttpService(HttpResponse response)
        : this((_, _) => Task.FromResult(response))
    {
    }

    public StubHttpService(Func<HttpRequest, CancellationToken, Task<HttpResponse>> sendAsync)
    {
        _sendAsync = sendAsync;
    }

    public List<HttpRequest> Requests { get; } = [];

    public List<CancellationToken> Tokens { get; } = [];

    public IWebProxy? Proxy { get; set; }

    public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
    {
        Requests.Add(request);
        Tokens.Add(token);
        return _sendAsync(request, token);
    }

    public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false)
    {
    }

    public Cookie? GetCookie(Uri uri, string name) => null;

    public IReadOnlyCollection<Cookie> GetCookies(Uri uri) => [];

    public IReadOnlyCollection<Cookie> GetAllCookies() => [];

    public void Dispose()
    {
    }
}

internal class PipelineHttpAction<T>(HttpResponse httpResponse) : HttpAction<T>
{
    public T? Result { get; init; }

    public bool EnsureSuccessStatusCodeValue { get; init; } = true;

    public Uri UriValue { get; init; } = new("https://example.com/api");

    public HttpMethod MethodValue { get; init; } = HttpMethod.Get;

    public Exception? GetResponseException { get; init; }

    public int ModifyRequestCallCount { get; private set; }

    public int GetResultCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public override IHttpService HttpService { get; } = new StubHttpService(httpResponse);

    public override Uri Uri => UriValue;

    public override HttpMethod Method => MethodValue;

    public override bool EnsureSuccessStatusCode => EnsureSuccessStatusCodeValue;

    public override Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
    {
        LastToken = token;
        return GetResponseException is null
            ? Task.FromResult(httpResponse)
            : Task.FromException<HttpResponse>(GetResponseException);
    }

    public override void ModifyRequest(HttpRequest request)
    {
        ModifyRequestCallCount++;
        request.SetHeader("X-Modified", "yes");
    }

    public override OperationResult<T> GetResult(HttpResponse response)
    {
        GetResultCallCount++;
        return Result!;
    }
}

internal sealed class ThrowingJsonHttpAction(HttpResponse httpResponse) : PipelineHttpAction<int>(httpResponse)
{
    public override OperationResult<int> GetResult(HttpResponse response)
    {
        return DefaultJsonAction.GetResult(new JsonCountAction(), response);
    }
}

internal sealed class JsonCountAction : JsonAction<int>
{
    public string? JsonPathValue { get; init; }

    public override string? JsonPath => JsonPathValue;
}

internal sealed class JsonStringAction : JsonAction<string>
{
    public string? JsonPathValue { get; init; }

    public override string? JsonPath => JsonPathValue;
}

internal sealed class UnitJsonAction : JsonAction
{
}

internal sealed class PipelineJsonAction<T>(HttpResponse response) : HttpJsonAction<T>
{
    public override IHttpService HttpService { get; } = new StubHttpService(response);

    public override Uri Uri { get; } = new("https://example.com/json");

    public override HttpMethod Method { get; } = HttpMethod.Get;

    public override Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
    {
        return Task.FromResult(response);
    }
}

internal sealed class XmlIntAction : XmlAction<int>
{
    public string? XPathValue { get; init; }

    public override string? XPath => XPathValue;
}

internal sealed class XmlStringAction : XmlAction<string>
{
    public string? XPathValue { get; init; }

    public override string? XPath => XPathValue;
}

internal sealed class UnitXmlAction : XmlAction
{
}

internal sealed class PipelineXmlAction<T>(HttpResponse response) : HttpXmlAction<T>
{
    public override IHttpService HttpService { get; } = new StubHttpService(response);

    public override Uri Uri { get; } = new("https://example.com/xml");

    public override HttpMethod Method { get; } = HttpMethod.Get;

    public override Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
    {
        return Task.FromResult(response);
    }
}

internal sealed class HtmlTextAction : HtmlAction<string>
{
    public string? HtmlSelectorValue { get; init; }

    public override string? HtmlSelector => HtmlSelectorValue;

    public override OperationResult<string> GetResult(HtmlActionContext context)
    {
        return Operation.Success(context.ResultElement?.TextContent ?? string.Empty);
    }
}

internal sealed class UnitHtmlAction : HtmlAction
{
}

internal sealed class TestHtmlFileAction(string filePath) : HtmlFileAction<string>
{
    public string? HtmlSelectorValue { get; init; }

    public override string FilePath { get; } = filePath;

    public override string? HtmlSelector => HtmlSelectorValue;

    public override OperationResult<string> GetResult(HtmlActionContext context)
    {
        return Operation.Success(context.ResultElement?.TextContent ?? string.Empty);
    }
}

internal sealed class TestJsonpAction(HttpResponse? response = null) : JsonpAction<JsonElement>
{
    public override IHttpService HttpService { get; } = new StubHttpService(response ?? HttpActionTestFixtures.CreateResponse());

    public override Uri Uri { get; } = new("https://example.com/jsonp");

    public override string CallbackParamName { get; } = "cb";

    public string CallbackNameValue { get; init; } = DefaultJsonpAction.DefaultCallbackName;

    public override string CallbackName => CallbackNameValue;

    public override Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
    {
        return Task.FromResult(response ?? HttpActionTestFixtures.CreateResponse());
    }
}

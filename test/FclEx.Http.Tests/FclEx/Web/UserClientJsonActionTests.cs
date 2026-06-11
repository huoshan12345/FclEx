namespace FclEx.Web;

public class UserClientJsonActionTests : WebTests
{
    [Fact]
    public void GetResult_WhenJsonPathMatches_DeserializesSelectedToken()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        var action = new CountJsonAction(client)
        {
            JsonPathValue = "data.count",
        };
        var response = CreateResponse("""{"data":{"count":3}}""");

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(3, result.Value);
    }

    [Fact]
    public void GetJson_WhenResponseIsNotJson_ReturnsError()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        var action = new CountJsonAction(client);
        var response = CreateResponse("not json");

        var result = action.GetJson(response);

        Assert.True(result.IsError);
        Assert.Contains("not a valid json", result.Exception.Message);
    }

    [Fact]
    public void CreateContext_WhenJsonPathMatches_ExposesSelectedTokens()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        var action = new CountJsonAction(client)
        {
            JsonPathValue = "items[*].id",
        };
        var response = CreateResponse();

        var result = action.CreateContext(response, """{"items":[{"id":1},{"id":2}]}""");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal([1, 2], result.Value!.ResultTokens.Select(m => m.GetInt32()));
    }

    [Fact]
    public async Task ExecuteAsync_WhenJsonParsingThrows_ReturnsPipelineError()
    {
        var response = CreateResponse("{ invalid-json }");
        var service = new CaptureHttpService(response);
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>())
        {
            HttpService = service,
        };
        var action = new CountJsonAction(client);

        var result = await ((FclEx.Actions.IAction<int>)action).ExecuteAsync();

        Assert.True(result.IsError);
        Assert.IsType<JsonException>(result.Exception, false);
    }

    private static FclEx.Http.HttpResponse CreateResponse(
        string responseString = "",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var request = FclEx.Http.HttpRequest.Get("https://example.com/json");
        var response = new FclEx.Http.HttpResponse(request);
        typeof(FclEx.Http.HttpResponse)
            .GetProperty(nameof(FclEx.Http.HttpResponse.ResponseString))!
            .SetValue(response, responseString);
        typeof(FclEx.Http.HttpResponse)
            .GetProperty(nameof(FclEx.Http.HttpResponse.StatusCode))!
            .SetValue(response, statusCode);
        return response;
    }

    private sealed class CountJsonAction(TestUserClient client)
        : UserClientJsonAction<TestUserClient, int>(client)
    {
        public string? JsonPathValue { get; init; }

        public override string? JsonPath => JsonPathValue;

        public override Uri Uri { get; } = new("https://example.com/json");

        public override HttpMethod Method { get; } = HttpMethod.Get;
    }

    private sealed class CaptureHttpService(FclEx.Http.HttpResponse response) : IHttpService
    {
        public Task<FclEx.Http.HttpResponse> SendAsync(FclEx.Http.HttpRequest request, CancellationToken token = default)
        {
            return Task.FromResult(response);
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false) { }

        public Cookie? GetCookie(Uri uri, string name) => null;

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri) => [];

        public IReadOnlyCollection<Cookie> GetAllCookies() => [];

        public IWebProxy? Proxy { get; set; }

        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose() { }
    }
}

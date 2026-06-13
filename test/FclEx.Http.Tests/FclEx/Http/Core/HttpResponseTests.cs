namespace FclEx.Http.Core;

public class HttpResponseTests : HttpServerTests
{
    [Fact]
    public void Constructor_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new HttpResponse(null!));

        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public void Constructor_InitializesDefaultResponseState()
    {
        var request = HttpRequest.Get("https://example.com/api");

        var response = new HttpResponse(request);

        Assert.Same(request, response.Request);
        Assert.True(response.IsSuccess);
        Assert.False(response.IsError);
        Assert.Null(response.Exception);
        Assert.Equal("", response.ResponseString);
        Assert.Empty(response.ResponseBytes);
        Assert.Same(Stream.Null, response.ResponseStream);
        Assert.Null(response.Encoding);
        Assert.Equal(default, response.Elapsed);
        Assert.Equal(default, response.StartTime);
        Assert.Equal(default, response.EndTime);
        Assert.Empty(response.Headers);
        Assert.Equal(default, response.StatusCode);
        Assert.Empty(response.VisitedUris);
    }

    [Fact]
    public void EndTime_ReturnsStartTimePlusElapsed()
    {
        var response = CreateResponse("");
        var start = new DateTimeOffset(2026, 6, 12, 1, 2, 3, TimeSpan.Zero);
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.StartTime))!
            .SetValue(response, start);
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.Elapsed))!
            .SetValue(response, TimeSpan.FromSeconds(4));

        Assert.Equal(start + TimeSpan.FromSeconds(4), response.EndTime);
    }

    [Fact]
    public void FromError_CreatesErrorResponseWithRequestAndException()
    {
        var request = HttpRequest.Get("https://example.com/api");
        var exception = new InvalidOperationException("broken");

        var response = HttpResponse.FromError(request, exception);

        Assert.Same(request, response.Request);
        Assert.Same(exception, response.Exception);
        Assert.False(response.IsSuccess);
        Assert.True(response.IsError);
    }

    [Fact]
    public void EnsureSuccessStatusCode_WhenStatusCodeIsSuccessful_ReturnsResponse()
    {
        var response = CreateResponse("", "https://example.com/api");
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.StatusCode))!
            .SetValue(response, HttpStatusCode.NoContent);

        var result = response.EnsureSuccessStatusCode();

        Assert.Same(response, result);
    }

    [Fact]
    public void EnsureSuccessStatusCode_WhenStatusCodeIsFailure_ThrowsWithRequestContext()
    {
        var response = CreateResponse("", "https://example.com/api");
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.StatusCode))!
            .SetValue(response, HttpStatusCode.BadRequest);

        var ex = Assert.Throws<HttpRequestException>(() => response.EnsureSuccessStatusCode());

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Contains("BadRequest/400", ex.Message);
        Assert.Contains("GET https://example.com/api", ex.Message);
    }

    [Fact]
    public void ThrowIfError_WhenResponseHasNoException_ReturnsResponse()
    {
        var response = CreateResponse("");

        var result = response.ThrowIfError();

        Assert.Same(response, result);
    }

    [Fact]
    public void AddCookies_AppendsSetCookieHeadersAndReturnsResponse()
    {
        var response = CreateResponse("");

        var result = response.AddCookies(["sid=abc", "theme=dark"]);

        Assert.Same(response, result);
        Assert.Equal(["sid=abc", "theme=dark"], response.Headers.Get(HttpHeaderNames.SetCookie));
    }

    [Fact]
    public void TryGetMediaType_WhenMultipleContentTypesExist_ReturnsLastParsableValue()
    {
        var response = CreateResponse("");
        response.Headers.Add(HttpHeaderNames.ContentType, "not a media type");
        response.Headers.Add(HttpHeaderNames.ContentType, "application/json; charset=utf-8");

        var found = response.TryGetMediaType(out var mediaType);

        Assert.True(found);
        Assert.NotNull(mediaType);
        Assert.Equal(MediaTypes.Json, mediaType.MediaType);
        Assert.Equal("utf-8", mediaType.CharSet);
    }

    [Fact]
    public void TryGetMediaType_WhenContentTypeIsMissing_ReturnsFalse()
    {
        var response = CreateResponse("");

        var found = response.TryGetMediaType(out var mediaType);

        Assert.False(found);
        Assert.Null(mediaType);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenPathMatches_ReturnsDeserializedValue()
    {
        var response = CreateResponse("""{"data":{"count":3}}""");

        var actual = response.ReadJsonAs<int>("data.count");

        Assert.True(actual.IsSuccess, actual.Exception?.ToString());
        Assert.Equal(3, actual.Value);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenPathDoesNotMatch_ReturnsError()
    {
        var response = CreateResponse("""{"data":{"count":3}}""");

        var actual = response.ReadJsonAs<int>("missing.count");

        Assert.True(actual.IsError);
        Assert.Contains("missing.count", actual.Exception!.Message);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenJsonIsMalformed_ReturnsError()
    {
        var response = CreateResponse("""{"data":}""");

        var actual = response.ReadJsonAs<int>("data.count");

        Assert.True(actual.IsError);
        Assert.Contains(actual.Exception.EnumerateInner(), m => m is JsonException);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenResponseHasException_ReturnsThatException()
    {
        var exception = new InvalidOperationException("network failed");
        var response = HttpResponse.FromError(HttpRequest.Get("https://example.com/api"), exception);

        var actual = response.ReadJsonAs<int>();

        Assert.True(actual.IsError);
        Assert.Same(exception, actual.Exception);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenResponseStringIsNotJson_ReturnsError()
    {
        var response = CreateResponse("plain text");

        var actual = response.ReadJsonAs<int>();

        Assert.True(actual.IsError);
        Assert.Contains("non-JSON", actual.Exception!.Message);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenDeserializationFails_ReturnsError()
    {
        var response = CreateResponse("""{"data":{"count":"abc"}}""");

        var actual = response.ReadJsonAs<int>("data.count");

        Assert.True(actual.IsError);
        Assert.Contains(actual.Exception.EnumerateInner(), m => m is JsonException);
    }

    [Fact]
    public void LastUri_WhenVisitedUrisIsEmpty_ThrowsClearError()
    {
        var response = CreateResponse("", addVisitedUri: false);

        var ex = Assert.Throws<InvalidOperationException>(() => response.LastUri());

        Assert.Contains("No visited URIs", ex.Message);
    }

    [Fact]
    public void GetDownloadInfo_WhenVisitedUrisIsEmpty_ThrowsClearError()
    {
        var response = CreateResponse("", addVisitedUri: false);

        var ex = Assert.Throws<InvalidOperationException>(() => response.GetDownloadInfo());

        Assert.Contains("No visited URIs", ex.Message);
    }

    [Fact]
    public void GetDownloadInfo_WhenFileNameEndsWithExtension_RemovesExactExtensionOnly()
    {
        var response = CreateResponse("", "https://example.com/download/report.zip.zip");

        var info = response.GetDownloadInfo();

        Assert.Equal("report.zip", info.FileNameWithoutExtension);
        Assert.Equal(".zip", info.FileExtension);
        Assert.Equal("report.zip.zip", info.FileName);
    }

    [Fact]
    public void GetDownloadInfo_WhenContentTypeHasKnownExtension_UsesMimeTypeExtension()
    {
        var response = CreateResponse("", "https://example.com/download/file");
        response.Headers.Add(HttpHeaderNames.ContentType, "image/jpg; charset=utf-8");

        var info = response.GetDownloadInfo();

        Assert.Equal("file", info.FileNameWithoutExtension);
        Assert.Equal(".jpg", info.FileExtension);
        Assert.Equal("image/jpg", info.MimeType);
    }

    [Fact]
    public void GetDownloadInfo_WhenUriHasNoFileName_UsesSanitizedHostAsBaseName()
    {
        var response = CreateResponse("payload", "https://example.com/");

        var info = response.GetDownloadInfo();

        Assert.Equal("example_com", info.FileNameWithoutExtension);
        Assert.Equal("", info.FileExtension);
        Assert.Equal("example_com", info.FileName);
    }

    [Fact]
    public void GetDownloadInfo_WhenBaseNameAndExtensionAreProvided_UsesProvidedValues()
    {
        var response = CreateResponse("payload", "https://example.com/download/report.bin");
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.ResponseBytes))!
            .SetValue(response, new byte[] { 1, 2, 3 });
        response.Headers.Add(HttpHeaderNames.ContentType, "application/octet-stream");

        var info = response.GetDownloadInfo("custom", ".dat");

        Assert.Equal("custom", info.FileNameWithoutExtension);
        Assert.Equal(".dat", info.FileExtension);
        Assert.Equal("custom.dat", info.FileName);
        Assert.Equal("application/octet-stream", info.MimeType);
        Assert.Equal([1, 2, 3], info.FileBytes);
    }

    [Fact]
    public void GetDownloadInfo_WhenUriHasNoExtension_UsesProvidedExtension()
    {
        var response = CreateResponse("payload", "https://example.com/download/report");

        var info = response.GetDownloadInfo("custom", ".dat");

        Assert.Equal("custom", info.FileNameWithoutExtension);
        Assert.Equal(".dat", info.FileExtension);
        Assert.Equal("custom.dat", info.FileName);
    }

    [Fact]
    public async Task DownloadAsync_WhenResponseIsSuccessful_BuildsDownloadRequestAndUsesSpecifiedFileName()
    {
        var service = new CaptureHttpService(request =>
        {
            var response = CreateResponse("", request.GetUri().ToString());
            response.Headers.Add(HttpHeaderNames.ContentType, "application/octet-stream");
            typeof(HttpResponse)
                .GetProperty(nameof(HttpResponse.ResponseBytes))!
                .SetValue(response, new byte[] { 1, 2, 3 });
            return response;
        });
        using var content = new TrackingContent();
        using var cts = new CancellationTokenSource();
        var options = new DownloadOptions
        {
            Uri = new Uri("https://example.com/download/original.bin"),
            Method = HttpMethod.Post,
            Content = content,
            BufferSize = 8192,
            ReadHeadersTimeout = TimeSpan.FromSeconds(3),
            ReadBufferTimeout = TimeSpan.FromSeconds(4),
            CancellationToken = cts.Token,
            FileBaseName = "specified",
            FileExtension = ".dat",
        };

        var result = await service.DownloadAsync(options);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("specified", result.Value.FileNameWithoutExtension);
        Assert.Equal(".dat", result.Value.FileExtension);
        Assert.Equal("specified.dat", result.Value.FileName);
        Assert.Equal([1, 2, 3], result.Value.FileBytes);
        Assert.Equal("application/octet-stream", result.Value.MimeType);
        Assert.NotNull(service.Request);
        Assert.Equal(options.Uri, service.Request.GetUri());
        Assert.Equal(HttpMethod.Post, service.Request.Method);
        Assert.Same(content, service.Request.Content);
        Assert.Equal(HttpContentType.Bytes, service.Request.ResponseContentType);
        Assert.Equal(options.BufferSize, service.Request.BufferSize);
        Assert.Equal(options.ReadHeadersTimeout, service.Request.ReadHeadersTimeout);
        Assert.Equal(options.ReadBufferTimeout, service.Request.ReadBufferTimeout);
        Assert.Contains("gzip", service.Request.Headers.Get(HttpHeaderNames.AcceptEncoding)!);
        Assert.Equal(cts.Token, service.Token);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task DownloadAsync_WhenResponseIsError_ReturnsErrorAndDisposesContent()
    {
        var exception = new InvalidOperationException("download failed");
        var service = new CaptureHttpService(request => HttpResponse.FromError(request, exception));
        using var content = new TrackingContent();
        var options = new DownloadOptions
        {
            Uri = new Uri("https://example.com/download/report.bin"),
            Content = content,
        };

        var result = await service.DownloadAsync(options);

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception!.InnerException);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task TaskThrowIfError_WhenResponseTaskReturnsError_ThrowsResponseException()
    {
        const string error = nameof(TaskThrowIfError_WhenResponseTaskReturnsError_ThrowsResponseException);
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return HttpResponse.FromError(HttpRequest.Get("http://localhost"), new SimpleException(error));
        }).ThrowIfError();

        var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
        Assert.Equal(error, ex.Message);
    }

    [Fact]
    public async Task TaskReadJsonAs_WhenResponseTaskSucceeds_DeserializesJson()
    {
        if (HasApiServer == false)
            return;

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var actual = await HttpRequest.Post("api/post")
            .JsonContent(expected)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ReadJsonAs<Dictionary<string, string>>();

        Assert.True(actual.IsSuccess, actual.Exception?.ToString());
        Assert.Equal(expected, actual.Value);
    }


    [Fact]
    public async Task TaskReadJsonAsRequired_WhenResponseTaskSucceeds_ReturnsDeserializedJson()
    {
        if (HasApiServer == false)
            return;

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var actual = await HttpRequest.Post("api/post")
            .JsonContent(expected)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ReadJsonAsRequired<Dictionary<string, string>>();

        Assert.Equal(expected, actual);
    }

    private static HttpResponse CreateResponse(string responseString, string requestUri = "https://example.com/api", bool addVisitedUri = true)
    {
        var response = new HttpResponse(HttpRequest.Get(requestUri));
        if (addVisitedUri)
        {
            response.VisitedUris.Add(new Uri(requestUri));
        }
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.ResponseString))!
            .SetValue(response, responseString);
        return response;
    }

    private sealed class CaptureHttpService(Func<HttpRequest, HttpResponse> responseFactory) : IHttpService
    {
        public HttpRequest? Request { get; private set; }
        public CancellationToken Token { get; private set; }
        public IWebProxy? Proxy { get; set; }
        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
        {
            Request = request;
            Token = token;
            return Task.FromResult(responseFactory(request));
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false)
        {
        }

        public Cookie? GetCookie(Uri uri, string name)
        {
            return null;
        }

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri)
        {
            return [];
        }

        public IReadOnlyCollection<Cookie> GetAllCookies()
        {
            return [];
        }

        public void Dispose()
        {
        }
    }

    private sealed class TrackingContent : ByteArrayContent
    {
        public bool IsDisposed { get; private set; }

        public TrackingContent() : base([1, 2, 3])
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}

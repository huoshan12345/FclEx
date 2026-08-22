namespace FclEx.Http.Extensions;

public class LoggerPropertiesExtensionsTests
{
    [Fact]
    public void Push_WithHttpRequestMessage_PushesRequestPropertiesToLoggerScopes()
    {
        var logger = new CaptureLogger();
        using var properties = new LoggerProperties(logger);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/items?id=1")
        {
            Content = new StringContent("payload", Encoding.UTF8, MediaTypes.Json),
        };

        var actual = properties.Push(request);

        var values = logger.Scopes
            .SelectMany(m => m)
            .ToDictionary(m => m.Key, m => m.Value);
        Assert.Same(properties, actual);
        Assert.Equal("/api/items", values[LogPropertyNames.RequestPath]);
        Assert.Equal("example.com", values[nameof(Uri.Host)]);
        Assert.Equal(request.Content.Headers.ContentType, values[nameof(HttpContentHeaders.ContentType)]);
        Assert.Equal(7L, values[nameof(HttpContentHeaders.ContentLength)]);
        Assert.Equal(HttpMethod.Post, values[nameof(HttpRequestMessage.Method)]);
    }

    [Fact]
    public void Push_WhenRequestHasNoUriOrContent_PushesNullUriAndContentValues()
    {
        var logger = new CaptureLogger();
        using var properties = new LoggerProperties(logger);
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
        };

        properties.Push(request);

        var values = logger.Scopes
            .SelectMany(m => m)
            .ToDictionary(m => m.Key, m => m.Value);
        Assert.Null(values[LogPropertyNames.RequestPath]);
        Assert.Null(values[nameof(Uri.Host)]);
        Assert.Null(values[nameof(HttpContentHeaders.ContentType)]);
        Assert.Null(values[nameof(HttpContentHeaders.ContentLength)]);
        Assert.Equal(HttpMethod.Delete, values[nameof(HttpRequestMessage.Method)]);
    }

    private sealed class CaptureLogger : ILogger
    {
        public List<IReadOnlyList<KeyValuePair<string, object?>>> Scopes { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> properties)
            {
                Scopes.Add(properties.ToArray());
            }

            return new ScopeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class ScopeDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

namespace FclEx.Http.Handlers;

public class LoggingDelegatingHandlerTests
{
    [Fact]
    public async Task SendAsync_WhenRequestSucceeds_LogsOnce()
    {
        var loggerProvider = new ListLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider));
        using var handler = new LoggingDelegatingHandler(loggerFactory)
        {
            InnerHandler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
        };
        using var invoker = new HttpMessageInvoker(handler);

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(loggerProvider.Entries);
        Assert.Equal(LogLevel.Information, loggerProvider.Entries[0].Level);
        Assert.Null(loggerProvider.Entries[0].Exception);
    }

    [Fact]
    public async Task SendAsync_WhenRequestFails_LogsOnceWithException()
    {
        var expected = new InvalidOperationException("boom");
        var loggerProvider = new ListLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider));
        using var handler = new LoggingDelegatingHandler(loggerFactory)
        {
            InnerHandler = new StubHandler(_ => throw expected),
        };
        using var invoker = new HttpMessageInvoker(handler);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None));

        Assert.Same(expected, actual);
        Assert.Single(loggerProvider.Entries);
        Assert.Equal(LogLevel.Warning, loggerProvider.Entries[0].Level);
        Assert.Same(expected, loggerProvider.Entries[0].Exception);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handle) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(handle(request));
        }
    }

    private sealed class ListLoggerProvider : ILoggerProvider
    {
        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new ListLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class ListLogger(List<LogEntry> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(new(logLevel, exception, formatter(state, exception)));
        }
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();

        public void Dispose()
        {
        }
    }

    private sealed record LogEntry(LogLevel Level, Exception? Exception, string Message);
}

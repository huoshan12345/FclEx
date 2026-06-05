namespace FclEx.Web;

public class UserClientLoggerTests
{
    [Fact]
    public void Log_WhenStateIsFormattedLogValues_PrefixesOriginalMessageFormat()
    {
        var logger = new CaptureLogger();
        using var factory = new CaptureLoggerFactory(logger);
        var client = new TestUserClient(factory)
        {
            Account = new UserAccount("alice", "pwd"),
        };

        client.Logger.LogInformation("Hello {Value}", 42);

        Assert.Equal("[alice]Hello 42", Assert.Single(logger.Messages));
    }

    [Fact]
    public void Log_WhenAccountNameIsEmpty_DoesNotPrefixMessage()
    {
        var logger = new CaptureLogger();
        using var factory = new CaptureLoggerFactory(logger);
        var client = new TestUserClient(factory)
        {
            Account = new UserAccount("", "pwd"),
        };

        client.Logger.LogInformation("Hello {Value}", 42);

        Assert.Equal("Hello 42", Assert.Single(logger.Messages));
    }

    private sealed class CaptureLoggerFactory(ILogger logger) : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => logger;

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class CaptureLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return Disposable.Empty;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}

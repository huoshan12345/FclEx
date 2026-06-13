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

    [Fact]
    public void Log_WhenStateIsNotFormattedLogValues_PrefixesFormattedMessage()
    {
        var logger = new CaptureLogger();
        using var factory = new CaptureLoggerFactory(logger);
        var client = new TestUserClient(factory)
        {
            Account = new UserAccount("alice", "pwd"),
        };

        client.Logger.Log(
            LogLevel.Information,
            new EventId(7, "event"),
            "raw-message",
            null,
            static (state, _) => state);

        Assert.Equal("[alice]raw-message", Assert.Single(logger.Messages));
    }

    [Fact]
    public void Constructor_WhenInnerLoggerIsAlreadyUserClientLogger_DoesNotPrefixTwice()
    {
        var logger = new CaptureLogger();
        using var factory = new CaptureLoggerFactory(logger);
        var client = new TestUserClient(factory)
        {
            Account = new UserAccount("alice", "pwd"),
        };
        var wrappedAgain = new UserClientLogger<IUserAccount>(client.Logger, client);

        wrappedAgain.LogInformation("Hello");

        Assert.Equal("[alice]Hello", Assert.Single(logger.Messages));
    }

    [Fact]
    public void BeginScope_DelegatesToInnerLogger()
    {
        var logger = new CaptureLogger();
        using var factory = new CaptureLoggerFactory(logger);
        var client = new TestUserClient(factory);
        var scope = KeyValuePair.Create("scope", (object?)"value");

        using (client.Logger.BeginScope(scope))
        {
        }

        Assert.Equal(scope, Assert.Single(logger.Scopes));
        Assert.Equal(1, logger.DisposedScopeCount);
    }

    [Fact]
    public void IsEnabled_DelegatesToInnerLogger()
    {
        var logger = new CaptureLogger
        {
            Enabled = false,
        };
        using var factory = new CaptureLoggerFactory(logger);
        var client = new TestUserClient(factory);

        var enabled = client.Logger.IsEnabled(LogLevel.Information);

        Assert.False(enabled);
        Assert.Equal(LogLevel.Information, logger.LastIsEnabledLevel);
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

        public List<object> Scopes { get; } = [];

        public int DisposedScopeCount { get; private set; }

        public bool Enabled { get; init; } = true;

        public LogLevel? LastIsEnabledLevel { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            Scopes.Add(state);
            return Disposable.Create(() => DisposedScopeCount++);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            LastIsEnabledLevel = logLevel;
            return Enabled;
        }

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

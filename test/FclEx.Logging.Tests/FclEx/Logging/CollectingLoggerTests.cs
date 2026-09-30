namespace FclEx.Logging;

public class CollectingLoggerTests
{
    [Fact]
    public void Log_None_DoesNotInvokeFormatterOrCollectEntry()
    {
        var logger = new CollectingLogger();
        var formatterCalled = false;

        logger.Log(LogLevel.None, default, "message", null, (state, exception) =>
        {
            formatterCalled = true;
            return state;
        });

        Assert.False(logger.IsEnabled(LogLevel.None));
        Assert.False(formatterCalled);
        Assert.Empty(logger.Entries);
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Log_EnabledLevel_PreservesEntryData(LogLevel level)
    {
        var logger = new CollectingLogger("category");
        var state = new object();
        var exception = new InvalidOperationException("failure");
        var eventId = new EventId(42, "event");

        logger.Log(level, eventId, state, exception, (_, _) => "message");

        var entry = Assert.Single(logger.Entries);
        Assert.True(logger.IsEnabled(level));
        Assert.Equal("category", entry.Category);
        Assert.Equal(level, entry.Level);
        Assert.Equal(eventId, entry.EventId);
        Assert.Equal("message", entry.Message);
        Assert.Same(state, entry.State);
        Assert.Same(exception, entry.Exception);
        Assert.Empty(entry.Scopes);
    }

    [Fact]
    public void BeginScope_NestedScopes_AreCapturedInOrderAndRestoredOnDispose()
    {
        var logger = new CollectingLogger();

        using (logger.BeginScope("outer"))
        {
            using (logger.BeginScope("inner"))
            {
                logger.LogInformation("nested");
            }

            logger.LogInformation("outer only");
        }

        logger.LogInformation("no scopes");

        Assert.Equal(new object?[] { "outer", "inner" }, logger.Entries[0].Scopes);
        Assert.Equal(new object?[] { "outer" }, logger.Entries[1].Scopes);
        Assert.Empty(logger.Entries[2].Scopes);
    }

    [Fact]
    public void Log_PropertyHelpers_CaptureStructuredScopes()
    {
        var logger = new CollectingLogger();

        using (logger.PushProperty("request", 42))
        using (logger.Properties("operation", "save"))
        {
            logger.With("user", "alice").LogInformation("message");
            logger.LogInformation("after wrapper");
        }

        var scopes = logger.Entries[0].Scopes;
        Assert.Equal(3, scopes.Count);
        AssertScopeProperty(scopes[0], "request", 42);
        AssertScopeProperty(scopes[1], "operation", "save");
        AssertScopeProperty(scopes[2], "user", "alice");
        Assert.Equal(2, logger.Entries[1].Scopes.Count);
    }

    [Fact]
    public async Task BeginScope_ConcurrentAsyncFlows_DoNotLeakBetweenOperations()
    {
        var logger = new CollectingLogger();
        var bothStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = 0;

        async Task WriteAsync(string scope)
        {
            using (logger.BeginScope(scope))
            {
                if (Interlocked.Increment(ref started) == 2)
                    bothStarted.SetResult(true);

                await bothStarted.Task;
                logger.LogInformation("{Scope}", scope);
            }
        }

        using (logger.BeginScope("parent"))
        {
            await Task.WhenAll(WriteAsync("first"), WriteAsync("second"));
            logger.LogInformation("parent only");
        }

        Assert.Equal(3, logger.Entries.Count);
        Assert.Equal(new object?[] { "parent", "first" },
            Assert.Single(logger.Entries, entry => entry.Message == "first").Scopes);
        Assert.Equal(new object?[] { "parent", "second" },
            Assert.Single(logger.Entries, entry => entry.Message == "second").Scopes);
        Assert.Equal(new object?[] { "parent" },
            Assert.Single(logger.Entries, entry => entry.Message == "parent only").Scopes);
    }

    [Fact]
    public void BeginScope_SeparateLoggers_DoNotShareScopes()
    {
        var first = new CollectingLogger();
        var second = new CollectingLogger();

        using (first.BeginScope("first"))
        {
            second.LogInformation("message");
        }

        Assert.Empty(Assert.Single(second.Entries).Scopes);
    }

    private static void AssertScopeProperty(object? scope, string key, object value)
    {
        var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(scope);
        var property = Assert.Single(properties);
        Assert.Equal(key, property.Key);
        Assert.Equal(value, property.Value);
    }
}

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

    [Fact]
    public void Clear_PreservesSnapshotsAndActiveScopes()
    {
        var logger = new CollectingLogger();
        Assert.Empty(logger.Entries);
        logger.Clear();

        using (logger.BeginScope("scope"))
        {
            logger.LogInformation("before clear");
            var snapshot = logger.Entries;
            logger.Clear();
            Assert.Empty(logger.Entries);
            logger.LogInformation("after clear");
            Assert.Equal("before clear", Assert.Single(snapshot).Message);
            Assert.Equal(new object?[] { "scope" }, Assert.Single(logger.Entries).Scopes);
        }
    }

    [Fact]
    public void Log_FormatterThrows_DoesNotAppendAnEntry()
    {
        var logger = new CollectingLogger();
        var error = new InvalidOperationException("formatter failed");
        var actual = Assert.Throws<InvalidOperationException>(() =>
            logger.Log(LogLevel.Information, default, "state", null, (_, _) => throw error));
        Assert.Same(error, actual);
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public void Log_MutableStateAndScope_AreRetainedByReference()
    {
        var logger = new CollectingLogger();
        var state = new List<string> { "before" };
        var scope = new Dictionary<string, object?> { ["key"] = "before" };
        using (logger.BeginScope(scope))
        {
            logger.Log(LogLevel.Information, default, state, null, (value, _) => value[0]);
        }

        state[0] = "after";
        scope["key"] = "after";
        var entry = Assert.Single(logger.Entries);
        Assert.Equal("before", entry.Message);
        Assert.Same(state, entry.State);
        Assert.Same(scope, Assert.Single(entry.Scopes));
    }

    [Fact]
    public async Task Log_ConcurrentWritesAndSnapshots_PreserveEveryEntry()
    {
        var logger = new CollectingLogger();
        var writers = Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            for (var i = 0; i < 50; i++)
            {
                logger.LogInformation("{Id}", worker * 50 + i);
                _ = logger.Entries;
            }
        }));
        await Task.WhenAll(writers);

        var entries = logger.Entries;
        Assert.Equal(400, entries.Count);
        Assert.Equal(400, entries.Select(entry => entry.Message).Distinct().Count());
    }

    private static void AssertScopeProperty(object? scope, string key, object value)
    {
        var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(scope);
        var property = Assert.Single(properties);
        Assert.Equal(key, property.Key);
        Assert.Equal(value, property.Value);
    }
}

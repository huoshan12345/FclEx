namespace FclEx.Logging;

public class LoggerPropertyScopeTests
{
    [Fact]
    public void PushAndDispose_PreserveNestedScopeOrderAndRestoreOuterScope()
    {
        var logger = new CollectingLogger();
        using (logger.BeginScope("outer"))
        {
            using (var scope = logger.Properties("First", 1))
            {
                Assert.Same(scope, scope.Push("Second", null));
                logger.LogInformation("nested");
            }
            logger.LogInformation("after");
        }

        Assert.Equal(3, logger.Entries[0].Scopes.Count);
        Assert.Equal("outer", logger.Entries[0].Scopes[0]);
        var first = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(logger.Entries[0].Scopes[1]);
        var second = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(logger.Entries[0].Scopes[2]);
        Assert.Equal(new KeyValuePair<string, object?>("First", 1), Assert.Single(first));
        Assert.Equal(new KeyValuePair<string, object?>("Second", null), Assert.Single(second));
        Assert.Equal(new object?[] { "outer" }, logger.Entries[1].Scopes);
    }

    [Fact]
    public void Dispose_RepeatedCalls_DoNotDisturbLaterScopes()
    {
        var logger = new CollectingLogger();
        var scope = new LoggerPropertyScope(logger, "First", 1);
        scope.Dispose();
        using (logger.BeginScope("later"))
        {
            scope.Dispose();
            logger.LogInformation("message");
        }
        Assert.Equal(new object?[] { "later" }, Assert.Single(logger.Entries).Scopes);
    }

    [Fact]
    public void Constructor_EmptyGroup_DoesNotStartScope()
    {
        var logger = new CollectingLogger();
        using (logger.Properties())
            logger.LogInformation("message");
        Assert.Empty(Assert.Single(logger.Entries).Scopes);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Equal("logger", Assert.Throws<ArgumentNullException>(() => new LoggerPropertyScope(null!)).ParamName);
    }

    [Fact]
    public void Push_NullOrEmptyName_ThrowsWithoutStartingScope()
    {
        var logger = new CollectingLogger();
        using var scope = new LoggerPropertyScope(logger);
        Assert.Throws<ArgumentNullException>(() => scope.Push(null!, 1));
        Assert.Throws<ArgumentException>(() => scope.Push("", 1));
        logger.LogInformation("message");
        Assert.Empty(Assert.Single(logger.Entries).Scopes);
    }
}

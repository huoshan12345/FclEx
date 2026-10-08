namespace FclEx.Logging.Extensions;

public class LoggerExtensionsTests
{
    [Fact]
    public void PushProperty_Iterator_IsEnumeratedOnceAndScopeIsStable()
    {
        var logger = new CollectingLogger();
        var calls = 0;
        IEnumerable<KeyValuePair<string, object?>> GetProperties()
        {
            calls++;
            yield return new("Value", calls);
        }

        using (logger.PushProperty(GetProperties()))
            logger.LogInformation("message");

        var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(
            Assert.Single(Assert.Single(logger.Entries).Scopes));
        Assert.Equal(1, Assert.Single(properties).Value);
        Assert.Equal(1, Assert.Single(properties).Value);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PushProperty_NullOrEmpty_DoesNotPushScope(bool useNull)
    {
        var logger = new CollectingLogger();
        using (logger.PushProperty(useNull ? null! : Array.Empty<KeyValuePair<string, object?>>()))
            logger.LogInformation("message");

        Assert.Empty(Assert.Single(logger.Entries).Scopes);
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    [InlineData(LogLevel.None)]
    public void LogOperationError_UsesRequestedLevelAndPreservesMetadata(LogLevel level)
    {
        var logger = new CollectingLogger();
        var error = new InvalidOperationException("failure");

        logger.LogOperationError(error, "save", TimeSpan.FromSeconds(2), level);
        if (level == LogLevel.None)
        {
            Assert.Empty(logger.Entries);
            return;
        }

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(level, entry.Level);
        Assert.Same(error, entry.Exception);
        Assert.Contains("save", entry.Message);
        Assert.Contains("failure", entry.Message);
        var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(Assert.Single(entry.Scopes));
        Assert.Equal(LogPropertyNames.DurationSeconds, Assert.Single(properties).Key);
        Assert.Equal(TimeSpan.FromSeconds(2).ToSecondsString(), Assert.Single(properties).Value);
        logger.LogInformation("after");
        Assert.Empty(logger.Entries[1].Scopes);
    }
}

namespace FclEx.Serilog.Extensions;

public partial class LogEventExtensionsTests
{
    [Theory]
    [InlineData(LogEventLevel.Warning, null, true)]
    [InlineData(LogEventLevel.Warning, LogEventLevel.Warning, true)]
    [InlineData(LogEventLevel.Warning, LogEventLevel.Error, true)]
    [InlineData(LogEventLevel.Error, LogEventLevel.Warning, false)]
    public void MatchMaxLevelOrNull_UsesInclusiveMaximum(LogEventLevel level, LogEventLevel? maximum, bool matches)
    {
        var logEvent = SerilogHelper.CreateLogEvent(level, null, "message");

        Assert.Equal(matches, logEvent.MatchMaxLevelOrNull(maximum));
    }

    [Fact]
    public void UnwrapException_ErrorOnlyMatchesPlaceholderName_PreservesException()
    {
        var exception = new SimpleException("Method");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception,
            "Failed to execute {Method}", "Save");

        logEvent.UnwrapException();

        Assert.Same(exception, logEvent.Exception);
    }

    [Fact]
    public void UnwrapException_LiteralMessageContainsError_RemovesException()
    {
        var exception = new SimpleException("remote service");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception,
            "Failed because the remote service is unavailable");

        logEvent.UnwrapException();

        Assert.Null(logEvent.Exception);
    }

    [Fact]
    public void UnwrapException_InnerMessageContainsError_ReplacesWithInner()
    {
        var inner = new InvalidOperationException("The remote service is unavailable");
        var exception = new SimpleException("remote service", inner);
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception, "operation failed");

        logEvent.UnwrapException();

        Assert.Same(inner, logEvent.Exception);
    }

    [Fact]
    public void UnwrapException_ExceptionHasStackTrace_PreservesException()
    {
        var exception = new SimpleException("failure", noStackTrace: false);
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception, "failure");
        Assert.False(string.IsNullOrEmpty(exception.StackTrace));

        logEvent.UnwrapException();

        Assert.Same(exception, logEvent.Exception);
    }

    [Fact]
    public void UnwrapException_MessageDoesNotContainError()
    {
        const string method = nameof(UnwrapException_MessageDoesNotContainError);
        var ex = new SimpleException("Test error message.");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, ex,
            "Failed to execute {Method}", method);

        var e = logEvent.UnwrapException();
        Assert.Equal(e, logEvent);
        Assert.NotNull(e.Exception);

        var message = e.MessageTemplate.Render(e.Properties);
        Assert.Equal($"Failed to execute \"{method}\"", message);
    }

    [Fact]
    public void UnwrapException_MessageContainsError()
    {
        const string method = nameof(UnwrapException_MessageContainsError);
        var ex = new SimpleException("Test error message.");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, ex,
            "Failed to execute {Method} due to {Error}", method, ex.Message);

        var e = logEvent.UnwrapException();
        Assert.Equal(e, logEvent);
        Assert.Null(e.Exception);

        var message = e.MessageTemplate.Render(e.Properties);
        Assert.Equal($"Failed to execute \"{method}\" due to \"{ex.Message}\"", message);
    }

    [Fact]
    public void UnwrapException_SetMessageTemplate()
    {
        var ex = new SimpleException("Test error message.");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, ex, "");

        var e = logEvent.UnwrapException();
        Assert.Equal(e, logEvent);
        Assert.Null(e.Exception);

        var message = e.MessageTemplate.Render(e.Properties);
        Assert.Equal(ex.Message, message);
    }
}

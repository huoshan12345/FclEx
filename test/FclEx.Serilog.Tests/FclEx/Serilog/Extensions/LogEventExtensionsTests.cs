namespace FclEx.Serilog.Extensions;

public class LogEventExtensionsTests
{
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
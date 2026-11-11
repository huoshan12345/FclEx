using System.Globalization;

namespace FclEx.Serilog.Extensions;

public partial class LogEventExtensionsTests
{
    [Theory]
    [InlineData(LogEventLevel.Verbose)]
    [InlineData(LogEventLevel.Debug)]
    [InlineData(LogEventLevel.Information)]
    [InlineData(LogEventLevel.Warning)]
    [InlineData(LogEventLevel.Error)]
    [InlineData(LogEventLevel.Fatal)]
    public void SetLevel_ChangesLevelAndReturnsSameEvent(LogEventLevel level)
    {
        var exception = new Exception("failure");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, exception, "Hello {Name}", "Ada");
        var template = logEvent.MessageTemplate;
        Assert.Same(logEvent, logEvent.SetLevel(level));
        Assert.Equal(level, logEvent.Level);
        Assert.Same(exception, logEvent.Exception);
        Assert.Same(template, logEvent.MessageTemplate);
        Assert.Equal("Ada", Assert.IsType<ScalarValue>(logEvent.Properties["Name"]).Value);
    }

    [Fact]
    public void SetException_ReplacesAndClearsException()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Warning, null, "message");
        var exception = new InvalidOperationException("failure");
        Assert.Same(logEvent, logEvent.SetException(exception));
        Assert.Same(exception, logEvent.Exception);
        Assert.Same(logEvent, logEvent.SetException(null));
        Assert.Null(logEvent.Exception);
    }

    [Fact]
    public void SetMessageTemplate_RetainsPropertiesAndUsesReplacementTokens()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Hello {Name}", "Ada");
        var property = logEvent.Properties["Name"];
        var template = new MessageTemplateParser().Parse("Goodbye {Name}");
        Assert.Same(logEvent, logEvent.SetMessageTemplate(template));
        Assert.Same(template, logEvent.MessageTemplate);
        Assert.Same(property, logEvent.Properties["Name"]);
        Assert.Equal("Goodbye \"Ada\"", logEvent.RenderMessage());
    }

    [Theory]
    [InlineData(null, "Hello \"Ada\"")]
    [InlineData("l", "Hello Ada")]
    [InlineData("j", "Hello \"Ada\"")]
    public void RenderMessage_UsesMessageTokenFormatAndLeavesWriterOpen(string? format, string expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Hello {Name}", "Ada");
        using var writer = new StringWriter();
        writer.Write("prefix:");
        LogEventExtensions.RenderMessage(logEvent, writer, format, CultureInfo.InvariantCulture);
        writer.Write(":suffix");
        Assert.Equal("prefix:" + expected + ":suffix", writer.ToString());
        Assert.Equal(expected, LogEventExtensions.RenderMessage(logEvent, format, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void RenderMessage_PassesFormatProviderToNumericTokens()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Value {Number:N2}", 1234.5);
        Assert.Equal("Value 1.234,50", LogEventExtensions.RenderMessage(logEvent, "l", CultureInfo.GetCultureInfo("de-DE")));
    }
}

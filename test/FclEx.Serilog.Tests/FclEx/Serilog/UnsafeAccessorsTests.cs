#if NET8_0_OR_GREATER
using System.Globalization;

namespace FclEx.Serilog;

public class UnsafeAccessorsTests
{
    [Fact]
    public void EventReferences_UpdatePublicGetters()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "before");
        var exception = new Exception("failure");
        var template = new MessageTemplateParser().Parse("after");
        UnsafeAccessors.LogEvent_Level(logEvent) = LogEventLevel.Fatal;
        UnsafeAccessors.LogEvent_Exception(logEvent) = exception;
        UnsafeAccessors.LogEvent_MessageTemplate(logEvent) = template;
        Assert.Equal(LogEventLevel.Fatal, logEvent.Level);
        Assert.Same(exception, logEvent.Exception);
        Assert.Same(template, logEvent.MessageTemplate);
        UnsafeAccessors.LogEvent_Exception(logEvent) = null;
        Assert.Null(logEvent.Exception);
    }

    [Fact]
    public void TemplateTextReference_DoesNotReparseTokens()
    {
        var template = new MessageTemplateParser().Parse("before {Name}");
        var tokens = template.Tokens.ToArray();
        UnsafeAccessors.MessageTemplate_Text(template) = "after";
        Assert.Equal("after", template.Text);
        Assert.Equal(tokens, template.Tokens);
    }

    [Fact]
    public void SinkListReference_AddsSinkToLoggerConfiguration()
    {
        var configuration = new LoggerConfiguration();
        var sink = new CollectingSink();
        UnsafeAccessors.LoggerConfiguration_Sinks(configuration).Add(sink);
        using var logger = configuration.CreateLogger();
        logger.Information("message");
        Assert.Single(sink.Events);
    }

#if NET10_0_OR_GREATER
    [Fact]
    public void InternalRenderer_WritesFormattedMessage()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Hello {Name}", "Ada");
        using var writer = new StringWriter();
        UnsafeAccessors.MessageTemplateRenderer_Render(null, logEvent.MessageTemplate, logEvent.Properties,
            writer, "l", CultureInfo.InvariantCulture);
        Assert.Equal("Hello Ada", writer.ToString());
    }
#endif
}
#endif

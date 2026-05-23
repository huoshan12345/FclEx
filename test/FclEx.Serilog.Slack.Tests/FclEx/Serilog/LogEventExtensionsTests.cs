using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace FclEx.Serilog;

public class LogEventExtensionsTests
{
    private static readonly Logger DefaultLogger = new LoggerConfiguration().CreateLogger();

    [MessageTemplateFormatMethod("messageTemplate")]
    public static LogEvent CreateLogEvent(LogEventLevel level, Exception? ex, string messageTemplate, params object?[] propertyValues)
    {
        Assert.True(DefaultLogger.BindMessageTemplate(messageTemplate, propertyValues, out var template, out var properties));
        properties = properties.Append(new(Constants.SourceContext, new ScalarValue(nameof(SlackSinkTests))));
        var date = DateTime.UtcNow.Date;
        var time = Random.Shared.NextDateTime(date, date.AddDays(1));
        return new LogEvent(time, level, ex, template, properties);
    }

    [Fact]
    public void RenderMessage_Test()
    {
        var logEvent = CreateLogEvent(LogEventLevel.Information, null, "Message from {Name}", "Tom");
        var writer = new StringWriter();
        logEvent.RenderMessage(writer, "l");
        var str = writer.ToString();
        Assert.Equal("Message from Tom", str);
    }
}
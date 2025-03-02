namespace FclEx.Serilog;

public static class SerilogHelper
{
    private static readonly Logger DefaultLoggerImpl = new LoggerConfiguration().CreateLogger();

    [MessageTemplateFormatMethod("messageTemplate")]
    public static LogEvent CreateLogEvent(LogEventLevel level, Exception? ex, string messageTemplate, params object?[] propertyValues)
    {
        Assert.True(DefaultLoggerImpl.BindMessageTemplate(messageTemplate, propertyValues, out var template, out var properties));
        properties = properties.Append(new(Constants.SourceContext, new ScalarValue(nameof(JsonFormatterTests))));
        var date = DateTime.UtcNow.Date;
        var time = Random.Shared.NextDateTime(date, date.AddDays(1));
        return new LogEvent(time, level, ex, template, properties);
    }
}
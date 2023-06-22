namespace FclEx.Serilog;

public static class LogEventHelper
{
    public static LogEvent Create(LogEventLevel level, Exception? exception, MessageTemplate messageTemplate,
        DateTimeOffset? timestamp = null, IEnumerable<LogEventProperty>? properties = null)
    {
        return new LogEvent(timestamp ?? DateTimeOffset.UtcNow, level, exception,
            messageTemplate, properties ?? Enumerable.Empty<LogEventProperty>());
    }

    public static LogEvent Create(LogEventLevel level, Exception? exception, string message,
        DateTimeOffset? timestamp = null, IEnumerable<LogEventProperty>? properties = null)
    {
        return Create(level, exception, new MessageTemplate(message, Enumerable.Empty<MessageTemplateToken>()),
            timestamp, properties);
    }
}
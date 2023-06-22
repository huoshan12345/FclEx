using Serilog.Formatting;

namespace FclEx.Serilog;

public static class AbpSerilogExtensions
{
    public static string Format(this ITextFormatter formatter, LogEvent logEvent)
    {
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
        var sw = new StringWriter(disposable.Value);
        formatter.Format(logEvent, sw);
        return sw.ToString();
    }

    public static string ToString(this LogEvent logEvent, ITextFormatter formatter)
    {
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
        var sw = new StringWriter(disposable.Value);
        formatter.Format(logEvent, sw);
        var str = sw.ToString();
        return str;
    }

    public static string Render(this LogEventPropertyValue value)
    {
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
        var sw = new StringWriter(disposable.Value);
        value.Render(sw, "l");
        var str = sw.ToString();
        return str;
    }

    public static string Render(this PropertyToken token, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
    {
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
        var sw = new StringWriter(disposable.Value);
        token.Render(properties, sw);
        var str = sw.ToString();
        return str;
    }

    public static LogEvent TryAddPropIfValid(this LogEvent logEvent, string? key, string? value)
    {
        if (key.IsValid() && value.IsValid())
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty(key, new ScalarValue(value)));
        }
        return logEvent;
    }
}
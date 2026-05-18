using NewRelic.Api.Agent;

namespace FclEx.NewRelic;

using static global::NewRelic.Api.Agent.NewRelic;

public static class NewRelicHelper
{
    public static ITransaction GetCurrentTransaction()
    {
        return GetAgent().CurrentTransaction;
    }

    public static ISpan GetCurrentSpan()
    {
        return GetAgent().CurrentSpan;
    }

    public static ITransaction AddCustomAttribute(this ITransaction transaction, IDictionary<string, object> dict)
    {
        foreach (var (key, value) in dict)
            transaction.AddCustomAttribute(key, value);
        return transaction;
    }

    public static ISpan AddCustomAttribute(this ISpan span, IDictionary<string, object> dict)
    {
        foreach (var (key, value) in dict)
            span.AddCustomAttribute(key, value);
        return span;
    }

    public static void RecordCustomEventSafely(string eventType, IEnumerable<KeyValuePair<string, object>> attributes)
    {
        try
        {
            var dic = attributes.ToDictionary(m => m.Key, m => GetValue(m.Value));
            RecordCustomEvent(eventType, dic);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to record email process event due to {ex.Message}");
        }
    }

    // NOTICE: The only attribute types accepted by the backend are string and float
    public static object GetValue(object? value)
    {
        if (value == null)
            return string.Empty;

        var type = value.GetType();

        if (type == typeof(string))
            return value;

        if (type.IsNumeric())
            return (float)(dynamic)value;

        return value switch
        {
            DateTime dt => dt.ToString("o"),
            DateTimeOffset dto => dto.ToString("o"),
            _ => value.ToString()!
        };
    }
}
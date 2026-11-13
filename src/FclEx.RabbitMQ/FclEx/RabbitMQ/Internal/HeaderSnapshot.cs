using System.Collections;
using System.Collections.ObjectModel;

namespace FclEx.RabbitMQ.Internal;

internal static class HeaderSnapshot
{
    internal static bool IsReserved(string key)
        => key is "x-opt-delivery-time" or "x-opt-deferral-token" or "x-delay" or "x-delayed-type"
            or "x-death" or "x-delivery-count" or "x-acquired-count" or "d-error-times"
            || key.StartsWith("x-first-death-", StringComparison.Ordinal)
            || key.StartsWith("x-last-death-", StringComparison.Ordinal)
            || key == Constants.PublishSequenceNumberHeader;

    internal static Dictionary<string, object?> Copy(IEnumerable<KeyValuePair<string, object?>>? values, bool rejectReserved = false)
        => Copy(values, rejectReserved, 0);

    internal static IReadOnlyDictionary<string, object?> ReadOnly(IEnumerable<KeyValuePair<string, object?>> values)
        => new ReadOnlyDictionary<string, object?>(Copy(values));

    private static Dictionary<string, object?> Copy(IEnumerable<KeyValuePair<string, object?>>? values, bool rejectReserved, int depth)
    {
        if (depth > 16)
            throw new ArgumentException("AMQP field tables cannot be cyclic or nested more than 16 levels.", nameof(values));
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (values is null)
            return result;
        foreach (var entry in values)
        {
            OptionsSnapshot.ShortString(entry.Key, nameof(values), allowEmpty: false);
            if (rejectReserved && IsReserved(entry.Key))
                throw new ArgumentException($"Header '{entry.Key}' is owned by the transport.", nameof(values));
            result.Add(entry.Key, CopyValue(entry.Value, depth + 1));
        }
        return result;
    }

    private static object? CopyValue(object? value, int depth)
    {
        if (depth > 16)
            throw new ArgumentException("AMQP field arrays cannot be cyclic or nested more than 16 levels.", nameof(value));
        if (value is null or string or bool or byte or sbyte or short or int or long or float or double or decimal or AmqpTimestamp)
            return value;
        if (value is byte[] bytes)
            return bytes.ToArray();
        if (value is IReadOnlyDictionary<string, object?> readOnly)
            return Copy(readOnly, false, depth);
        if (value is IDictionary<string, object?> dictionary)
            return Copy(dictionary, false, depth);
        if (value is IList list)
        {
            var copy = new object?[list.Count];
            for (var i = 0; i < list.Count; i++)
                copy[i] = CopyValue(list[i], depth + 1);
            return copy;
        }
        throw new ArgumentException($"Unsupported AMQP field value type: {value.GetType()}.", nameof(value));
    }
}

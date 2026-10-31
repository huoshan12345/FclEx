namespace FclEx.Extensions;

public static class ReadOnlyDictionaryExtensions
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? Get<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
    {
        return dic.TryGetValue(key, out var value) ? value : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TMember? Get<TKey, TValue, TMember>(
        this IReadOnlyDictionary<TKey, TValue> dic,
        TKey key,
        Func<TValue, TMember> selector,
        TMember? defaultValue = default)
        where TMember : class
    {
        return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TMember? Get<TKey, TValue, TMember>(
        this IReadOnlyDictionary<TKey, TValue> dic,
        TKey key,
        Func<TValue, TMember> selector,
        TMember? defaultValue = default)
        where TMember : struct
    {
        return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
    }

    public static bool TryGetInt<TKey>(
        this IReadOnlyDictionary<TKey,
#nullable disable
        string
#nullable restore
        > dic,
        TKey key,
        out int value,
        NumberStyles style = NumberStyles.Integer,
        IFormatProvider? provider = null)
    {
        if (dic.TryGetValue(key, out var str)
            && int.TryParse(str, style, provider, out value))
            return true;

        value = default;
        return false;
    }

    /// <summary>Reads and parses a dictionary value as a date and time with an offset.</summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <param name="dic">The dictionary containing the date text.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">The parsed value, or default when the key is missing or parsing fails.</param>
    /// <param name="format">An exact date format; null uses general date parsing. A failed exact match does not fall back to general parsing.</param>
    /// <param name="formatProvider">The culture used to parse the value; null uses the current culture.</param>
    /// <param name="styles">The date parsing options. Invalid combinations can throw ArgumentException.</param>
    /// <returns>True when the key exists and its value is successfully parsed; otherwise false.</returns>
    public static bool TryGetDateTimeOffset<TKey>(
        this IReadOnlyDictionary<TKey,
#nullable disable
        string
#nullable restore
        > dic,
        TKey key,
        out DateTimeOffset value,
        [StringSyntax("DateTimeFormat")] string? format = null,
        IFormatProvider? formatProvider = null,
        DateTimeStyles styles = DateTimeStyles.None)
    {
        if (dic.TryGetValue(key, out var str))
        {
            if (format is not null && DateTimeOffset.TryParseExact(str, format, formatProvider, styles, out value))
                return true;

            if (format is null && DateTimeOffset.TryParse(str, formatProvider, styles, out value))
                return true;
        }

        value = default;
        return false;
    }

    public static OperationResult<TValue> GetValueResult<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key)
    {
        return dic.TryGetValue(key, out var value)
            ? value is null
                ? $"Key '{key}' has null value"
                : Operation.Success(value)
            : $"Key '{key}' not found";
    }
}

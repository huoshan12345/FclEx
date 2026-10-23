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
        if (dic.TryGetValue(key, out var str)
            && DateTimeOffset.TryParseExact(str, format, formatProvider, styles, out value))
            return true;

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

namespace FclEx.Utils;

public static class PooledStringBuilderExtensions
{
    public static T RenderBlock<T>(this T builder, string quote, Action<T> action, string? endQuote = null)
        where T : PooledStringBuilder<T>, new()
    {
        endQuote ??= quote;
        builder.Append(quote);
        action(builder);
        builder.Append(endQuote);
        return builder;
    }

    public static T Clear<T>(this T builder) where T : PooledStringBuilder<T>, new()
    {
        builder.StringBuilder.Clear();
        return builder;
    }

    public static T Append<T>(this T builder, char value) where T : PooledStringBuilder<T>, new()
    {
        builder.StringBuilder.Append(value);
        return builder;
    }

    public static T Append<T>(this T builder, string? value) where T : PooledStringBuilder<T>, new()
    {
        builder.StringBuilder.Append(value);
        return builder;
    }
}
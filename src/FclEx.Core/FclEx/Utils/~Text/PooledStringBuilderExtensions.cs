namespace FclEx.Utils;

public static class PooledStringBuilderExtensions
{
    public static T Clear<T>(this T builder) where T : PooledStringBuilder<T>, new()
    {
        builder.Builder.Clear();
        return builder;
    }

    public static T Append<T>(this T builder, string? value) where T : PooledStringBuilder<T>, new()
    {
        return builder.Append(m => m.Append(value));
    }

    public static T Append<T>(this T builder, char value) where T : PooledStringBuilder<T>, new()
    {
        return builder.Append(m => m.Append(value));
    }

    public static T Append<T>(this T builder, Action<StringBuilder> valueAction) where T : PooledStringBuilder<T>, new()
    {
        valueAction(builder.Builder);
        return builder;
    }
}
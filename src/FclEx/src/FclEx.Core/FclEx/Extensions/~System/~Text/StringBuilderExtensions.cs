namespace FclEx.Extensions;

public static class StringBuilderExtensions
{
#if NETSTANDARD2_0
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string? separator, IEnumerable<T> values)
    {
        Check.NotNull(builder);
        Check.NotNull(values);

        using var e = values.GetEnumerator();

        if (!e.MoveNext())
            return builder;

        builder.Append(e.Current?.ToString());

        while (e.MoveNext())
        {
            builder.Append(separator);
            builder.Append(e.Current?.ToString());
        }

        return builder;
    }
#endif
}
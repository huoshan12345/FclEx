namespace System.Collections.Generic;

public static class ComparerBuilderExtensions
{
    public static ComparerBuilder<T> Key<T, TKey>(this ComparerBuilder<T> builder, Func<T?, TKey?> keySelector, IComparer<TKey?>? keyComparer = null)
    {
        return builder.Set(KeyComparer.Create(keySelector, keyComparer));
    }

    public static ComparerBuilder<T> Common<T>(this ComparerBuilder<T> builder, Comparison<T?> comparison)
    {
        return builder.Set(CommonComparer.Create(comparison));
    }

    public static MemberComparerBuilder<T> Member<T>(this IComparerBuilder<T> _)
    {
        return MemberComparerBuilder.Create<T>();
    }

    public static ComparerBuilder<T> Reverse<T>(this ComparerBuilder<T> builder)
    {
        return builder.Set(m => new ReverseComparer<T>(m));
    }

    public static IComparer<IEnumerable<T>> ToEnumerable<T>(this IComparerBuilder<T> builder)
    {
        return EnumerableComparer.Create(builder.Build());
    }
}
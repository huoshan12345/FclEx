namespace System.Collections.Generic;

public static class EqualityComparerBuilderExtensions
{
    public static EqualityComparerBuilder<T> Key<T, TKey>(this EqualityComparerBuilder<T> builder, Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
    {
        return builder.Set(KeyEqualityComparer.Create(keySelector, keyComparer));
    }

    public static EqualityComparerBuilder<T> Common<T>(this EqualityComparerBuilder<T> builder, Func<T, T, bool> compareFunc, Func<T, int> hashFunc)
    {
        return builder.Set(CommonEqualityComparer.Create(compareFunc, hashFunc));
    }

    public static EqualityComparerBuilder<T> Reference<T>(this EqualityComparerBuilder<T> builder)
    {
        return builder.Set(ReferenceEqualityComparer<T>.Instance);
    }

    public static EqualityComparerBuilder<string> FileExtension<T>(this EqualityComparerBuilder<string> builder)
    {
        return builder.Set(FileExtensionEqualityComparer.Instance);
    }

    public static MemberEqualityComparerBuilder<T> Member<T>(this EqualityComparerBuilder<T> _)
    {
        return MemberEqualityComparerBuilder.Create<T>();
    }
}
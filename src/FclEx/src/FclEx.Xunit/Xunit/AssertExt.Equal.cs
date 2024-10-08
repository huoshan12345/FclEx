namespace Xunit;

partial class AssertExt
{
    public static void EveryMemberEqual<T>(T expected, T actual, params string[] excludeMemberPaths)
    {
        var tree = BuildExcludeMemberTree(excludeMemberPaths);
        var result = Equal(expected, actual, tree, false, null, null);
        result.ThrowIfNotEqual();
    }

    public static void NotEveryMemberEqual<T>(T expected, T actual, params string[] excludeMemberPaths)
    {
        var tree = BuildExcludeMemberTree(excludeMemberPaths);
        var result = Equal(expected, actual, tree, false, null, null);
        result.ThrowIfEqual();
    }

    public static void EverySameNameMemberEqual<T1, T2>(T1 expected, T2 actual, params string[] excludeMemberPaths)
    {
        var tree = BuildExcludeMemberTree(excludeMemberPaths);
        var result = Equal(expected, actual, tree, true, null, null);
        result.ThrowIfNotEqual();
    }

    public static void NotEverySameNameMemberEqual<T1, T2>(T1 expected, T2 actual, params string[] excludeMemberPaths)
    {
        var tree = BuildExcludeMemberTree(excludeMemberPaths);
        var result = Equal(expected, actual, tree, true, null, null);
        result.ThrowIfEqual();
    }

    public static void Equal(DateTime? expected, DateTime? actual, TimeSpan precision)
    {
        if (expected == null && actual == null)
            return;

        if (expected == null || actual == null)
            throw EqualException.ForMismatchedValues(expected, actual);

        Assert.Equal(expected.Value, actual.Value, precision);
    }

    public static void Equal(TimeSpan expected, TimeSpan actual, TimeSpan precision)
    {
        if (expected == actual)
            return;

        var diff = (expected - actual).Duration();
        if (diff < precision)
            return;

        throw EqualException.ForMismatchedValues(expected, actual);
    }

    public static void Contains<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> collection, TKey key, TValue value)
    {
        Assert.Contains(new(key, value), collection);
    }

    public static void NotEmpty([NotNull] string? value)
    {
        Assert.NotNull(value);
        Assert.NotEmpty(value);
    }

    public static void NotEmpty([NotNull] IEnumerable? value)
    {
        Assert.NotNull(value);
        Assert.NotEmpty(value);
    }

    public static void Equal<TEnum, TInt>(TEnum expected, TInt actual)
        where TEnum : struct, Enum
        where TInt : struct, IConvertible
    {
        Assert.Equal(expected.CastTo<TInt?>(), actual);
    }

    public static void Equal<TEnum, TInt>(TEnum? expected, TInt? actual)
        where TEnum : struct, Enum
        where TInt : struct, IConvertible
    {
        Assert.Equal(expected.CastTo<TInt?>(), actual);
    }

    public static void Equal<TEnum, TInt>(TInt? expected, TEnum actual)
        where TEnum : struct, Enum
        where TInt : struct, IConvertible
    {
        Assert.Equal(expected, actual.CastTo<TInt?>());
    }

    public static void Equal<TEnum, TInt>(TInt? expected, TEnum? actual)
        where TEnum : struct, Enum
        where TInt : struct, IConvertible
    {
        Assert.Equal(expected, actual.CastTo<TInt?>());
    }
}
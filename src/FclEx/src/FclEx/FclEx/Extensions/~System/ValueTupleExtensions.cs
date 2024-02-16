namespace FclEx.Extensions;

// ReSharper disable once PartialTypeWithSinglePart
public static partial class ValueTupleExtensions
{
    public static KeyValuePair<T1, T2> AsKeyValue<T1, T2>(this ValueTuple<T1, T2> tuple)
    {
        return KvPair.Create(tuple.Item1, tuple.Item2);
    }

    //[return: NotNullIfNotNull(nameof(defaultValue))]
    //public static string? FirstNotEmpty(this (string?, string?) tuple, string? defaultValue = "")
    //{
    //    const int count = 2;
    //    using var disposable = ArrayPool<string?>.Shared.GetAsDisposable(count);
    //    var arr = disposable.Value;
    //    arr[0] = tuple.Item1;
    //    arr[1] = tuple.Item2;
    //    return arr.FirstNotEmpty(count, defaultValue);
    //}

    //[return: NotNullIfNotNull(nameof(defaultValue))]
    //public static string? FirstNotEmpty(this (string?, string?, string?) tuple, string? defaultValue = "")
    //{
    //    const int count = 3;
    //    using var disposable = ArrayPool<string?>.Shared.GetAsDisposable(count);
    //    var arr = disposable.Value;
    //    arr[0] = tuple.Item1;
    //    arr[1] = tuple.Item2;
    //    arr[2] = tuple.Item3;
    //    return arr.FirstNotEmpty(count, defaultValue);
    //}

    //[return: NotNullIfNotNull(nameof(defaultValue))]
    //public static string? FirstNotEmpty(this (string?, string?, string?, string?) tuple, string? defaultValue = "")
    //{
    //    const int count = 4;
    //    using var disposable = ArrayPool<string?>.Shared.GetAsDisposable(count);
    //    var arr = disposable.Value;
    //    arr[0] = tuple.Item1;
    //    arr[1] = tuple.Item2;
    //    arr[2] = tuple.Item3;
    //    arr[3] = tuple.Item4;
    //    return arr.FirstNotEmpty(count, defaultValue);
    //}
}
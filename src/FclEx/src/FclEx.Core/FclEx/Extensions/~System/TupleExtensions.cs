namespace FclEx.Extensions;

public static class TupleExtensions
{
    public static (T1, T2) ToValue<T1, T2>(this Tuple<T1, T2> tuple)
        => (tuple.Item1, tuple.Item2);

    public static (T1, T2, T3) ToValue<T1, T2, T3>(this Tuple<T1, T2, T3> tuple)
        => (tuple.Item1, tuple.Item2, tuple.Item3);

    public static (T1, T2, T3, T4) ToValue<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> tuple)
        => (tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);

    public static IEnumerable<(T1, T2)> ToValue<T1, T2>(this IEnumerable<Tuple<T1, T2>> enumerable)
        => enumerable.Select(m => m.ToValue());

    public static IEnumerable<(T1, T2, T3)> ToValue<T1, T2, T3>(this IEnumerable<Tuple<T1, T2, T3>> enumerable)
        => enumerable.Select(m => m.ToValue());

    public static IEnumerable<(T1, T2, T3, T4)> ToValue<T1, T2, T3, T4>(this IEnumerable<Tuple<T1, T2, T3, T4>> enumerable)
        => enumerable.Select(m => m.ToValue());

}
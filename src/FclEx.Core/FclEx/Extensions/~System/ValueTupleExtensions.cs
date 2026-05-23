namespace FclEx.Extensions;

public static partial class ValueTupleExtensions
{
    public static KeyValuePair<T1, T2> ToKeyValuePair<T1, T2>(this ValueTuple<T1, T2> tuple)
    {
        return KeyValuePair.Create(tuple.Item1, tuple.Item2);
    }
}
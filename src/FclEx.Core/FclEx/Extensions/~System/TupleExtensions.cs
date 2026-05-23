namespace FclEx.Extensions;

public static partial class TupleExtensions
{
    public static KeyValuePair<T1, T2> ToKeyValuePair<T1, T2>(this Tuple<T1, T2> tuple)
    {
        return KeyValuePair.Create(tuple.Item1, tuple.Item2);
    }
}
namespace FclEx.Extensions;

public static partial class ValueTupleExtensions
{
    public static KeyValuePair<T1, T2> AsKeyValue<T1, T2>(this ValueTuple<T1, T2> tuple)
    {
        return KvPair.Create(tuple.Item1, tuple.Item2);
    }
}
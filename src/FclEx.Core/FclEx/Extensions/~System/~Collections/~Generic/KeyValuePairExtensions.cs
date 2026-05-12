namespace FclEx.Extensions;

public static class KeyValuePairExtensions
{
    public static KeyValuePair<TKey, TValue> ValueOf<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, TValue value)
    {
        return KeyValuePair.Create(pair.Key, value);
    }

    public static Tuple<T1, T2> ToTuple<T1, T2>(this KeyValuePair<T1, T2> pair)
    {
        return Tuple.Create(pair.Key, pair.Value);
    }

    public static (T1, T2) ToValueTuple<T1, T2>(this KeyValuePair<T1, T2> pair)
    {
        return (pair.Key, pair.Value);
    }

#if !NET5_0_OR_GREATER
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
#endif
}
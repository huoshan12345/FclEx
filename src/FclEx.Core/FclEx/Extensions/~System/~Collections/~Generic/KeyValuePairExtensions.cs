namespace FclEx.Extensions;

public enum DupPolicy
{
    OnlyLast = 0,
    OnlyFirst = 1,
    Throw = 2,
    Array
}

public static class KeyValuePairExtensions
{
    public static JsonObject ToJsonObject(this IEnumerable<KeyValuePair<string, string>> pairs, DupPolicy policy = DupPolicy.OnlyLast)
    {
        var obj = new JsonObject();
        foreach (var pair in pairs.Where(m => m.Key != null).GroupBy(m => m.Key))
        {
            var values = pair.Select(m => m.Value).ToHashSet();
            if (values.Count > 0)
                obj.Add(pair.Key, values.ToJToken(policy));
        }
        return obj;
    }

    public static KeyValuePair<TKey, TValue> ValueOf<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, TValue value)
    {
        return KeyValuePair.Create(pair.Key, value);
    }

    public static (T1, T2) AsTuple<T1, T2>(this KeyValuePair<T1, T2> pair)
    {
        return (pair.Key, pair.Value);
    }

#if NETSTANDARD2_0
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
#endif
}
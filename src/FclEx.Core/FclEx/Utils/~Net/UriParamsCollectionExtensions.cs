namespace FclEx.Utils;

public static class UriParamsCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> AsKeyValuePairs(this UriParams collection)
    {
        return collection.Select(m => m.ToKeyValuePair());
    }

    public static UriParams Add(this UriParams collection, IEnumerable<KeyValuePair<string, string>> enumerable)
    {
        Check.NotNull(enumerable);
        foreach (var (key, value) in enumerable)
        {
            collection.Add(key, value);
        }
        return collection;
    }

    public static UriParams Add(this UriParams collection, UriParam parameter)
    {
        return collection.Add(parameter.Key, parameter.Value);
    }

    public static UriParams Add(this UriParams collection, IEnumerable<UriParam> enumerable)
    {
        foreach (var param in enumerable)
        {
            return collection.Add(param);
        }
        return collection;
    }
}
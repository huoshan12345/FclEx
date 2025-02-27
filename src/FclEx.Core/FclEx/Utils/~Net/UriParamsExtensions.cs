namespace FclEx.Utils;

public static class UriParamsExtensions
{
    public static UriParams Add<T>(this UriParams uriParams, string? key, T? value)
    {
        return uriParams.Add(key, value?.ToString());
    }

    public static UriParams Add(this UriParams uriParams, KeyValuePair<string?, string?> pair)
    {
        return uriParams.Add(pair.Key, pair.Value);
    }

    public static UriParams Add(this UriParams uriParams, Tuple<string?, string?> pair)
    {
        return uriParams.Add(pair.Item1, pair.Item2);
    }

    public static UriParams Add(this UriParams uriParams, (string?, string?) pair)
    {
        return uriParams.Add(pair.Item1, pair.Item2);
    }

    public static UriParams Add(this UriParams uriParams, IEnumerable<KeyValuePair<string, string>> enumerable)
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            uriParams.Add(key, value);
        }
        return uriParams;
    }

    public static UriParams Add(this UriParams uriParams, string pair, string separator = ":")
    {
        Check.NotNull(pair);
        var (key, value) = pair.Partition(separator);
        return uriParams.Add(key.Trim(), value.Trim());
    }
    
    public static UriParams Add<T>(this UriParams uriParams, T builder) where T : IUriParamsBuilder
    {
        return uriParams.Add(builder.Build());
    }
}
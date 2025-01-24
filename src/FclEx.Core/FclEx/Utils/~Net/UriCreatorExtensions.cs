namespace FclEx.Utils;

public static class UriCreatorExtensions
{
    public static UriCreator Scheme(this UriCreator creator, string scheme)
    {
        creator.Scheme = scheme;
        return creator;
    }

    public static UriCreator Host(this UriCreator creator, string host)
    {
        creator.Host = host;
        return creator;
    }

    public static UriCreator Port(this UriCreator creator, int port)
    {
        creator.Port = port;
        return creator;
    }

    public static UriCreator UserName(this UriCreator creator, string userName)
    {
        creator.UserName = userName;
        return creator;
    }

    public static UriCreator Path(this UriCreator creator, string path)
    {
        creator.Path = path;
        return creator;
    }

    public static UriCreator Fragment(this UriCreator creator, string fragment)
    {
        creator.Fragment = fragment;
        return creator;
    }

    public static UriCreator AddQueryParam(this UriCreator creator, string key, object? value)
    {
        creator.Query.Add(key, value);
        return creator;
    }

    public static UriCreator AddQueryParam(this UriCreator creator, IEnumerable<UriParam> enumerable)
    {
        creator.Query.Add(enumerable);
        return creator;
    }

    public static UriCreator AddQueryParam(this UriCreator creator, IEnumerable<KeyValuePair<string, string>> enumerable)
    {
        creator.Query.Add(enumerable);
        return creator;
    }
}
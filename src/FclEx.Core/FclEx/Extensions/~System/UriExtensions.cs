namespace FclEx.Extensions;

public static class UriExtensions
{
    public static Uri Combine(this Uri baseUri, Uri relativeUri)
    {
        return new Uri(baseUri, relativeUri);
    }

    public static Uri Combine(this Uri baseUri, string relativeUri)
    {
        return new Uri(baseUri, relativeUri);
    }

    public static Uri WithPath(this Uri uri, string path)
    {
        var builder = new UriBuilder(uri)
        {
            Path = path
        };
        return builder.Uri;
    }
}

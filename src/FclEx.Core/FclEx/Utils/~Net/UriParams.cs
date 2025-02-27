namespace FclEx.Utils;

public sealed class UriParams() : NameValues<UriParams>(StringComparer.Ordinal), IRenderable
{
    public override string ToString()
    {
        return this.RenderToString();
    }

    public void Render(StringBuilder builder)
    {
        foreach (var (_, (key, value), _, isLast) in this.IndexEx())
        {
            // see https://source.dot.net/#System.Web.HttpUtility/System/Web/HttpUtility.cs,e8f7afaff17514d9,references
            if (key.IsNotEmpty())
            {
                builder.Append(HttpUtility.UrlEncode(key));
                builder.Append('=');
            }

            if (value.IsNotEmpty())
            {
                builder.Append(HttpUtility.UrlEncode(value));
            }

            if (isLast == false)
                builder.Append('&');
        }
    }

    public static UriParams Parse(string? query)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        return new UriParams().Add(dic.Enumerate());
    }

    public static UriParams From(IEnumerable<KeyValuePair<string, string>> pairs) => new UriParams().Add(pairs);
    public static UriParams From<T>(string? key, T value) => new UriParams().Add(key, value);
}
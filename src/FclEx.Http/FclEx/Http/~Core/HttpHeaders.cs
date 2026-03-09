namespace FclEx.Http;

public class HttpHeaders() : NameValues<HttpHeaders>(StringComparer.OrdinalIgnoreCase), IRenderable
{
    public void Render(StringBuilder builder)
    {
        foreach (var (key, value) in this)
        {
            builder.Append(key);
            builder.Append(": ");
            builder.Append(value);
            builder.Append("\r\n");
        }
    }

    public override HttpHeaders Add(string? key, string? value)
    {
        // http headers do not allow empty key
        if (key.IsNullOrEmpty())
            return this;

        return base.Add(key, value);
    }

    public override HttpHeaders Set(string? key, string? value)
    {
        // http headers do not allow empty key
        if (key.IsNullOrEmpty())
            return this;

        // use null to remove header
        return value == null
            ? Remove(key)
            : base.Set(key, value);
    }

    public static HttpHeaders Parse(string? query)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        return new HttpHeaders().Add(dic.Enumerate());
    }

    public static HttpHeaders From(IEnumerable<KeyValuePair<string, string>> pairs) => new HttpHeaders().Add(pairs);
    public static HttpHeaders From<T>(string? key, T value) => new HttpHeaders().Add(key, value);
}
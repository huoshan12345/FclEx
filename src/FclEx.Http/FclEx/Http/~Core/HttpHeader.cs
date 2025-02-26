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

    public static HttpHeaders Parse(string? query)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        return new HttpHeaders().Add(dic.Enumerate());
    }

    public static HttpHeaders From(IEnumerable<KeyValuePair<string, string>> pairs) => new HttpHeaders().Add(pairs);
    public static HttpHeaders From<T>(string? key, T value) => new HttpHeaders().Add(key, value);
}
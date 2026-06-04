namespace FclEx.Http;

public static class HttpContentHeadersExtensions
{
    public static void CopyTo(this HttpContentHeaders headers, HttpContentHeaders other, params string[] excludeHeaders)
    {
        foreach (var (key, values) in headers)
        {
            if (excludeHeaders.Contains(key, StringComparer.OrdinalIgnoreCase))
                continue;

            other.TryAddWithoutValidation(key, values);
        }
    }
}
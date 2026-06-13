namespace FclEx.Http;

/// <summary>
/// Extensions for copying HTTP content headers.
/// </summary>
public static class HttpContentHeadersExtensions
{
    /// <summary>
    /// Copies headers to another <see cref="HttpContentHeaders"/> collection with validation disabled.
    /// Header names listed in <paramref name="excludeHeaders"/> are skipped case-insensitively.
    /// </summary>
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

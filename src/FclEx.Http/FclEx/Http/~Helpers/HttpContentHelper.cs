namespace FclEx.Http;

/// <summary>
/// Factory helpers for common <see cref="HttpContent"/> instances.
/// </summary>
public static class HttpContentHelper
{
    /// <summary>
    /// Serializes an object to JSON and returns UTF-8 JSON string content.
    /// </summary>
    public static StringContent ToJsonContent(object obj, JsonSerializerOptions? options = null)
    {
        var json = obj.ToJson(options);
        return FromJson(json);
    }

    /// <summary>
    /// Wraps an existing JSON string as UTF-8 JSON string content.
    /// </summary>
    public static StringContent FromJson(string json)
    {
        return new StringContent(json, Encoding.UTF8, MediaTypes.Json);
    }

    /// <summary>
    /// Creates UTF-8 string content and wraps it in GZip compression for sending.
    /// </summary>
    public static GZipContent ToGZipContent(string content, string contentType = MediaTypes.Text)
    {
        return new StringContent(content, Encoding.UTF8, contentType).ToGZip();
    }
}

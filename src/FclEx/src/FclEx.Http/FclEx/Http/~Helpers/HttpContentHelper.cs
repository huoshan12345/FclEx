namespace FclEx.Http;

public static class HttpContentHelper
{
    public static StringContent ToJsonContent(object obj, JsonSerializerOptions? options = null)
    {
        var json = obj.ToJson(options);
        return new StringContent(json, Encoding.UTF8, MimeTypes.Json);
    }

    public static GZipContent ToGZipContent(string content, string contentType = MimeTypes.Text)
    {
        return new StringContent(content, Encoding.UTF8, contentType).ToGZip();
    }
}
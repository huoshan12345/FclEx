namespace FclEx.Http;

public static class HttpContentHelper
{
    public static StringContent ToJsonContent(object obj, JsonSerializerOptions? options = null)
    {
        var json = obj.ToJson(options);
        return FromJson(json);
    }

    public static StringContent FromJson(string json)
    {
        return new StringContent(json, Encoding.UTF8, MediaTypes.Json);
    }

    public static GZipContent ToGZipContent(string content, string contentType = MediaTypes.Text)
    {
        return new StringContent(content, Encoding.UTF8, contentType).ToGZip();
    }
}
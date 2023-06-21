namespace FclEx.Http;

public static class HttpContentHelper
{
    public static StringContent ToJsonContent(object obj, JsonOptions options = default)
    {
        var json = obj.ToJson(options);
        return new StringContent(json, Encoding.UTF8, HttpMediaTypes.Json);
    }

    public static StreamContent ToGZipContent(string content, string? contentType = HttpMediaTypes.Text)
    {
        var stream = new MemoryStream();
        using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
        using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
        {
            contentStream.CopyTo(gzipStream);
        }
        stream.Position = 0; // NOTE: very important to reset the position
        return new StreamContent(stream)
        {
            Headers =
            {
                { "Content-Encoding", "gzip" },
                { "Content-Type", contentType },
            }
        };
    }
}
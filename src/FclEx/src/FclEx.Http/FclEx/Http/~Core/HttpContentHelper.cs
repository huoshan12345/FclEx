using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading;
using FclEx.Extensions;
using Newtonsoft.Json;

namespace FclEx.Http;

public static class HttpContentHelper
{
    public static StringContent ToJsonContent(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        return new StringContent(json, Encoding.UTF8, HttpMediaTypes.Json);
    }

    public static StreamContent ToGZipContent(ArraySegment<byte> bytes, string? contentType = HttpMediaTypes.Text)
    {
        var stream = new MemoryStream();
        using (var contentStream = bytes.ToMemoryStream())
        using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
        {
            contentStream.CopyTo(gzipStream);
        }
        stream.Position = 0; // NOTE: very important to reset the position
        return new StreamContent(stream)
        {
            Headers =
            {
                { HttpKnownHeaderNames.ContentEncoding, "gzip" },
                { HttpKnownHeaderNames.ContentType, contentType },
            }
        };
    }

    public static ArraySegmentContent ToArraySegmentContent(ArraySegment<byte> bytes, TimeSpan? timeout, string?
        contentType = HttpMediaTypes.Text, CancellationToken token = default)
    {
        return new ArraySegmentContent(bytes, timeout, token)
        {
            Headers = { { HttpKnownHeaderNames.ContentType, contentType }, }
        };
    }
}
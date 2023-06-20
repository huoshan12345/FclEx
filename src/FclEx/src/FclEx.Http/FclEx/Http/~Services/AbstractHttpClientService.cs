using System.Diagnostics;
using System.Net.Http.Headers;

namespace FclEx.Http;

public abstract class AbstractHttpClientService : AbstractHttpService
{
    protected static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

    protected static readonly string[] NotAddHeaderNames =
    {
        HttpKnownHeaderNames.ContentType,
        HttpKnownHeaderNames.Cookie,
        // HttpKnownHeaderNames.UserAgent
    };

    protected AbstractHttpClientService(bool useCookie, IWebProxy? proxy = null, ILoggerFactory? loggerFactory = null)
        : base(useCookie, proxy, loggerFactory)
    {
    }

    protected void ReadCookies(HttpResponseMessage responseMessage, HttpResponse response)
    {
        if (!responseMessage.Headers.TryGetValues(HttpKnownHeaderNames.SetCookie, out var cookies)) return;
        var arr = cookies.ToArray();
        if (arr.IsEmpty())
            return;

        response.Headers.AddRange(HttpKnownHeaderNames.SetCookie, arr);
        SaveCookies(responseMessage.RequestMessage?.RequestUri!, arr);
    }

    protected static void ReadHeader(HttpResponseMessage responseMessage, HttpResponse response)
    {
        foreach (var (key, values) in responseMessage.Headers.Where(m => m.Key != HttpKnownHeaderNames.SetCookie))
        {
            response.Headers.AddRange(key, values);
        }
    }

    protected static async Task ReadContentAsync(HttpResponseMessage responseMessage, HttpResponse response, CancellationToken token)
    {
        var request = response.Request;
        foreach (var (key, value) in responseMessage.Content.Headers)
        {
            response.Headers.AddRange(key, value);
        }

        switch (request.ReadType)
        {
            case HttpContentType.Stream:
            {
                response.ResponseStream = await responseMessage.Content.ReadAsStreamAsync(token);
                break;
            }
            case HttpContentType.Bytes:
            {
                response.ResponseBytes = await responseMessage.Content.ReadAsByteArrayAsync(request.BufferSize, request.ReadBufferTimeout, token);
                break;
            }
            case HttpContentType.String:
            {
                var bytes = await responseMessage.Content.ReadAsByteArrayAsync(request.BufferSize, request.ReadBufferTimeout, token);
                (response.ResponseString, response.Encoding) = ReadBufferAsString(bytes, responseMessage.Content.Headers, request.DetectCharSet, request.FallbackCharSet);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(request.ReadType), request.ReadType, null);
        }
    }

    protected static Encoding? GetEncodingFromCharSet(string? charset)
    {
        if (charset.IsNullOrEmpty())
            return null;

        try
        {
            // Remove at most a single set of quotes.
            if (charset!.Length > 2 &&
                charset[0] == '\"' &&
                charset[^1] == '\"')
            {
                return Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
            }
            else
            {
                return Encoding.GetEncoding(charset);
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"The character set '{charset}' provided in ContentType is invalid due to {ex.Message}", ex);
        }
    }

    protected static (string, Encoding) ReadBufferAsString(ArraySegment<byte> buffer, HttpContentHeaders headers, bool detectCharSet, string? defaultCharSet)
    {
        Debug.Assert(buffer.Array != null);

        var charSet = headers.ContentType?.CharSet;
        // We don't validate the Content-Encoding header: If the content was encoded, it's the caller's
        // responsibility to make sure to only call ReadAsString() on already decoded content. E.g. if the
        // Content-Encoding is 'gzip' the user should set HttpClientHandler.AutomaticDecompression to get a
        // decoded response stream.

        Encoding? encoding = null;
        var bomLength = -1;

        // If we do have encoding information in the 'Content-Type' header, use that information to convert
        // the content to a string.
        if (charSet.IsValid())
        {
            encoding = GetEncodingFromCharSet(charSet);
            // Byte-order-mark (BOM) characters may be present even if a charset was specified.
            bomLength = EncodingHelper.GetPreambleLength(buffer, encoding!);
        }

        // If no content encoding is listed in the ContentType HTTP header, or no Content-Type header present,
        // then check for a BOM in the data to figure out the encoding.
        if (encoding == null)
        {
            if (!EncodingHelper.TryDetectEncoding(buffer, out encoding, out bomLength))
            {
                // We already checked to see if the data had a UTF8 BOM in TryDetectEncoding
                // and DefaultStringEncoding is UTF8, so the bomLength is 0.
                bomLength = 0;

                if (detectCharSet)
                {
                    var media = headers.ContentType?.MediaType;
                    if (media != null && media.Contains("html")) // html or xhtml
                    {
                        encoding = DetectCharSet(buffer);
                    }
                }
            }
        }

        encoding ??= GetEncodingFromCharSet(defaultCharSet) ?? DefaultStringEncoding;

        // Drop the BOM when decoding the data.
        var str = encoding.GetString(buffer.Array, buffer.Offset + bomLength, buffer.Count - bomLength);
        return (str, encoding);
    }

    private static Encoding? DetectCharSet(ArraySegment<byte> buffer)
    {
        var data = buffer.Array ?? throw new ArgumentNullException(nameof(buffer.Array));
        if (data.Length == 0)
            return null;

        var prefix = Encoding.Default.GetString(data, 0, Math.Min(1024, data.Length));
        var charSet = HtmlUtil.GetMetaCharSet(prefix);
        return charSet == null ? null : Encoding.GetEncoding(charSet);
    }

    protected static HttpRequestMessage BuildHttpRequest(HttpRequest request, CookieContainer cc, CancellationToken token)
    {
        var requestMessage = new HttpRequestMessage(request.Method, request.GetUri());

        if (request.Method.IsGet() == false)
        {
            if (request.Content is { } content)
            {
                requestMessage.Content = content.ToBuffered(request.UseGZip, request.ReadBufferTimeout, request.BufferSize, token);
            }
            else if (request.FormValues.IsValid())
            {
                requestMessage.Content = new FormUrlEncodedContent(request.FormValues.AsEnumerable());
            }

            if (requestMessage.Content?.Headers is { ContentType: { } contentType })
            {
                contentType.CharSet ??= request.CharSet;

                if (request.MediaType is { } mediaType)
                {
                    contentType.MediaType ??= mediaType;
                }
            }
        }

        foreach (var (key, value) in request.Headers.Where(h => NotAddHeaderNames.Contains(h.Key) == false))
        {
            requestMessage.Headers.Add(key, value);
        }

        var cookies = request.Headers.Get(HttpKnownHeaderNames.Cookie);
        requestMessage.AddCookie(cookies);

        var cookiesInCc = cc.GetCookieHeader(requestMessage.RequestUri!);
        requestMessage.AddCookie(cookiesInCc);

        return requestMessage;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequest httpReq, CancellationToken token,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead)
    {
        var httpRequest = BuildHttpRequest(httpReq, _cookieContainer, token);
        var res = await httpClient.SendAsync(httpRequest, httpCompletionOption, token).DonotCapture();
        return res;
    }

    protected async Task ExecuteAsyncInternal(HttpClient httpClient, HttpRequest request, HttpResponse response, CancellationToken token = default)
    {
        using var cts = token.WithTimeout(request.TotalTimeout);
        var responseMessages = new List<HttpResponseMessage>();
        try
        {
            var curReq = request;
            while (true)
            {
                using var ctsPerReq = cts.Token.WithTimeout(request.ConnectTimeout);
                var res = await SendAsync(httpClient, curReq, ctsPerReq.Token).DonotCapture();
                responseMessages.Add(res);
                response.RedirectUris.Add(res.RequestMessage?.RequestUri!);
                if (request.ReadCookie)
                    ReadCookies(res, response);

                if (!res.TryGetRedirection(out var uri))
                    break;

                curReq = HttpRequest.Get(uri);
            }

            var responseMessage = responseMessages.Last(); // responses should not be empty
            response.StatusCode = responseMessage.StatusCode;

            if (request.ReadHeader)
                ReadHeader(responseMessage, response);

            if (request.ThrowIfFailed)
                responseMessage.EnsureSuccess();

            if (request.ReadContent)
                await ReadContentAsync(responseMessage, response, cts.Token).DonotCapture();
        }
        finally
        {
            cts.Dispose();
            responseMessages.ForEach(m => m?.Dispose());
            responseMessages.Clear();
        }
    }

}
namespace FclEx.Http;

public readonly record struct HttpClientContext(HttpClient Client, IAsyncPolicy<HttpResponseMessage> Policy);

public abstract class AbstractHttpClientService : AbstractHttpService
{
    protected static readonly Encoding DefaultEncoding = Encoding.UTF8;

    protected static readonly string[] NotAddHeaderNames =
    [
        HttpHeaderNames.ContentType,
        HttpHeaderNames.Cookie,
        HttpHeaderNames.ContentLength,
    ];

    protected void ReadCookies(HttpResponseMessage responseMessage, HttpResponse response)
    {
        if (responseMessage.TryGetCookies(out var cookies) == false)
            return;

        var arr = cookies.AsICollection();
        if (arr.IsEmpty())
            return;

        response.AddCookies(arr);
        SaveCookies(responseMessage.RequestMessage?.RequestUri!, arr);
    }

    protected static void ReadHeader(HttpResponseMessage responseMessage, HttpResponse response)
    {
        foreach (var (key, values) in responseMessage.Headers.Where(m => m.Key != HttpHeaderNames.SetCookie))
        {
            response.Headers.AddRange(key, values);
        }
    }

    protected virtual async Task ReadContentAsync(HttpResponseMessage responseMessage, HttpResponse response, CancellationToken token)
    {
        var request = response.Request;
        foreach (var (key, value) in responseMessage.Content.Headers)
        {
            response.Headers.AddRange(key, value);
        }

        switch (request.ReadContentType)
        {
            case HttpContentType.Stream:
            {
                var stream = await responseMessage.Content.ReadAsStreamAsync(token);
                response.ResponseStream = new HttpResponseStream(responseMessage, stream);
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
                (response.ResponseString, response.Encoding) = ReadBufferAsString(bytes, responseMessage.Content.Headers, request.CharSet, request.DetectCharSet, request.FallbackCharSet, request.IgnoreInvalidCharSet);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(request.ReadContentType), request.ReadContentType, null);
        }
    }

    protected Encoding? GetEncodingFromCharSet(string? charSet, bool ignoreInvalidCharSet)
    {
        if (charSet.IsNullOrEmpty())
            return null;

        try
        {
            return GetEncoding(charSet.Trim('\'').Trim('"'));
        }
        catch (Exception ex)
        {
            if (ignoreInvalidCharSet == false)
                throw new InvalidOperationException($"The character set '{charSet}' is invalid due to {ex.Message}", ex);

            return null;
        }
    }

    protected virtual Encoding GetEncoding(string charSet)
    {
        return charSet switch
        {
            "utf8" => Encoding.UTF8,
            _ => Encoding.GetEncoding(charSet),
        };
    }

    protected virtual (string, Encoding) ReadBufferAsString(Span<byte> buffer, HttpContentHeaders headers, string? charSet, bool detectCharSet, string? defaultCharSet, bool ignoreInvalidCharSet)
    {
        charSet = (charSet, headers.ContentType?.CharSet).FirstNotEmpty();
        // We don't validate the Content-Encoding header: If the content was encoded, it's the caller's
        // responsibility to make sure to only call ReadAsString() on already decoded content. E.g. if the
        // Content-Encoding is 'gzip' the user should set HttpClientHandler.AutomaticDecompression to get a
        // decoded response stream.

        Encoding? encoding = null;
        var bomLength = -1;

        // If we do have encoding information in the 'Content-Type' header, use that information to convert
        // the content to a string.
        if (charSet.IsNotEmpty())
        {
            encoding = GetEncodingFromCharSet(charSet, ignoreInvalidCharSet);

            if (encoding is not null)
            {
                // Byte-order-mark (BOM) characters may be present even if a charset was specified.
                bomLength = EncodingHelper.GetPreambleLength(buffer, encoding);
            }
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

        encoding ??= GetEncodingFromCharSet(defaultCharSet, ignoreInvalidCharSet) ?? DefaultEncoding;

        // Drop the BOM when decoding the data.
        var str = encoding.GetString(buffer[bomLength..]);
        return (str, encoding);
    }

    private static Encoding? DetectCharSet(Span<byte> data)
    {
        if (data.Length == 0)
            return null;

        var len = Math.Min(1024, data.Length);
        var prefix = Encoding.Default.GetString(data[..len]);
        var charSet = HtmlHelper.GetMetaCharSet(prefix);
        return charSet == null ? null : Encoding.GetEncoding(charSet);
    }

    protected static HttpRequestMessage BuildHttpRequest(HttpRequest request, Uri? baseAddress, CookieContainer cc, CancellationToken token)
    {
        var uri = request.GetUri();
        var requestMessage = new HttpRequestMessage(request.Method, uri)
        {
            Version = request.Version,
#if NET6_0_OR_GREATER
            VersionPolicy = request.VersionPolicy,
#endif
        };

        if (request.Method.IsGet() == false)
        {
            if (request.Content is { } content)
            {
                requestMessage.Content = content;
            }
            else if (request.Form.IsNotEmpty())
            {
                requestMessage.Content = new FormUrlEncodedContent(request.Form.AsKeyValuePairs());
            }

            if (requestMessage.Content is { } requestContent)
            {
                requestMessage.Content = requestContent.ToCompressed(request.CompressionMethod, request.CompressionLevel, request.ReadBufferTimeout, request.BufferSize, token);
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

        var cookies = request.Headers.Get(HttpHeaderNames.Cookie);
        foreach (var cookie in cookies)
        {
            requestMessage.AddCookie(cookie);
        }

        var cookieUri = uri.IsAbsoluteUri == false && baseAddress is not null
            ? baseAddress
            : uri;

        if (cookieUri.IsAbsoluteUri)
        {
            var cookiesInCc = cc.GetCookieHeader(cookieUri);
            requestMessage.AddCookie(cookiesInCc);
        }
        return requestMessage;
    }

    protected virtual async Task<HttpResponseMessage> SendAsync(HttpClientContext context, HttpRequest request, CancellationToken token)
    {
        var (client, policy) = context;
        var response = await policy.ExecuteAsync(async () =>
        {
            // Create request in every retry to avoid the following error:
            // The request message was already sent. Cannot send the same request message multiple times.
            using var httpRequest = BuildHttpRequest(request, client.BaseAddress, _cookieContainer, token);
            using var cts = token.WithTimeout(request.ReadHeadersTimeout);
            return await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        });
        return response;
    }

    // ReSharper disable once MemberCanBeProtected.Global
    protected internal abstract HttpClientContext CreateHttpClientContext();

    protected override async Task ExecuteAsyncInternal(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken token)
    {
        var cts = token.WithTimeout(httpRequest.TotalTimeout);
        var responses = new List<HttpResponseMessage>();
        try
        {
            var context = CreateHttpClientContext();
            var currentRequest = httpRequest;
            while (true)
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace(currentRequest.Dump(this));

                var response = await SendAsync(context, currentRequest, cts.Token);
                responses.Add(response);
                httpResponse.RedirectUris.Add(response.RequestMessage?.RequestUri!);

                if (httpRequest.ReadCookies)
                    ReadCookies(response, httpResponse);

                if (response.TryGetRedirection(out var uri) == false)
                    break;

                currentRequest = HttpRequest.Get(uri).SetHeader(currentRequest.Headers);
            }

            var last = responses.Last(); // responses should not be empty
            httpResponse.StatusCode = last.StatusCode;
            ReadHeader(last, httpResponse);

            if (httpRequest.EnsureSuccessStatusCode)
                last.EnsureSuccess();

            if (httpRequest.ReadContent)
            {
                await ReadContentAsync(last, httpResponse, cts.Token);

                if (httpRequest.ReadContentType == HttpContentType.Stream)
                {
                    // the last will be disposed in HttpResponse.ResponseStream instead of here.
                    responses.Remove(last);
                }
            }
        }
        finally
        {
            cts.Dispose();
            responses.ForEach(m => m.Dispose());
            responses.Clear();
        }
    }

}
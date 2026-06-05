namespace FclEx.Http;

public abstract class HttpClientServiceBase : HttpServiceBase
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
        SaveCookies(responseMessage.RequestMessage?.RequestUri, arr);
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

        switch (request.ResponseContentType)
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
                (response.ResponseString, response.Encoding) = ReadBufferAsString(
                    buffer: bytes,
                    headers: responseMessage.Content.Headers,
                    charSet: request.CharSet,
                    detectCharSet: request.DetectCharSet,
                    defaultCharSet: request.FallbackCharSet,
                    ignoreInvalidCharSet: request.IgnoreInvalidCharSet);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(request.ResponseContentType), request.ResponseContentType, null);
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

    protected virtual (string, Encoding) ReadBufferAsString(Span<byte> buffer, HttpContentHeaders headers,
        string? charSet, bool detectCharSet, string? defaultCharSet, bool ignoreInvalidCharSet)
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

    protected internal static HttpRequestMessage BuildHttpRequest(HttpRequest request, BufferedContent? content, Uri? baseAddress, CookieContainer cc, CancellationToken token)
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
            if (content is not null)
            {
                requestMessage.Content = content.CloneIfDisposed();
            }
            else if (request.Form.IsNotEmpty())
            {
                requestMessage.Content = new FormUrlEncodedContent(request.Form);
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

        if (request.Authorization is null && request.UserName.IsNotEmpty())
        {
            request.BasicAuth(request.UserName, request.Password);
        }

        foreach (var (key, value) in request.Headers.Where(h => h.Key.Length > 0 && NotAddHeaderNames.Contains(h.Key) == false))
        {
            if (request.AddHeaderWithoutValidation)
            {
                requestMessage.Headers.TryAddWithoutValidation(key, value);
            }
            else
            {
                requestMessage.Headers.Add(key, value);
            }
        }

        if (request.UseDefaultUserAgent && requestMessage.Headers.UserAgent is { Count: 0 } userAgent)
        {
            userAgent.ParseAdd(HttpConstants.DefaultUserAgent);
        }

        var cookies = request.Headers.GetValues(HttpHeaderNames.Cookie);
        foreach (var cookie in cookies.EmptyIfNull())
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

    protected internal static async Task<BufferedContent?> CreateBufferedContentAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var content = request.Content;
        return content switch
        {
            null => null,
            BufferedContent bufferedContent => bufferedContent,
            _ => await BufferedContent.CreateAsync(content, request.ReadBufferTimeout, request.BufferSize, cancellationToken),
        };
    }

    protected virtual async Task<HttpResponseMessage> SendAsync(HttpClientContext context, HttpRequest request, BufferedContent? bufferedContent, CancellationToken token)
    {
        var (client, policy, _) = context;
        var response = await policy.ExecuteAsync(async () =>
        {
            // Create request in every retry to avoid the following error:
            // The request message was already sent. Cannot send the same request message multiple times.
            using var httpRequest = BuildHttpRequest(request, bufferedContent, client.BaseAddress, _cookieContainer, token);
            using var cts = token.WithTimeout(request.ReadHeadersTimeout);
            return await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        });
        return response;
    }

    protected internal static HttpRequest CreateRedirectRequest(HttpRequest request, HttpResponseMessage response, Uri uri)
    {
        var preserveMethodAndContent = response.StatusCode.PreservesMethodAndContent();
        var method = preserveMethodAndContent || request.Method == HttpMethod.Head
            ? request.Method
            : HttpMethod.Get;

        var redirectRequest = HttpRequest.Create(uri, method);
        CopyRedirectOptions(request, redirectRequest, preserveMethodAndContent);
        return redirectRequest;
    }

    private static void CopyRedirectOptions(HttpRequest source, HttpRequest target, bool preserveContent)
    {
        target.EnsureSuccessStatusCode = source.EnsureSuccessStatusCode;
        target.Version = source.Version;
#if NET6_0_OR_GREATER
        target.VersionPolicy = source.VersionPolicy;
#endif
        target.BufferSize = source.BufferSize;
        target.TotalTimeout = source.TotalTimeout;
        target.ReadBufferTimeout = source.ReadBufferTimeout;
        target.ReadHeadersTimeout = source.ReadHeadersTimeout;
        target.MediaType = preserveContent ? source.MediaType : null;
        target.CharSet = preserveContent ? source.CharSet : null;
        target.DetectCharSet = source.DetectCharSet;
        target.FallbackCharSet = source.FallbackCharSet;
        target.IgnoreInvalidCharSet = source.IgnoreInvalidCharSet;
        target.CompressionMethod = preserveContent ? source.CompressionMethod : CompressionMethod.None;
        target.CompressionLevel = preserveContent ? source.CompressionLevel : CompressionLevel.NoCompression;
        target.ResponseContentType = source.ResponseContentType;
        target.ReadContent = source.ReadContent;
        target.ReadCookies = source.ReadCookies;
        target.UseDefaultUserAgent = source.UseDefaultUserAgent;
        target.AddHeaderWithoutValidation = source.AddHeaderWithoutValidation;
        target.MaxRedirectCount = source.MaxRedirectCount;
        target.AllowInsecureRedirects = source.AllowInsecureRedirects;

        foreach (var (key, value) in source.Headers)
        {
            if (ShouldCopyRedirectHeader(key))
            {
                target.Headers.Add(key, value);
            }
        }

        if (preserveContent)
        {
            target.Form.Add(source.Form);
        }
    }

    private static bool ShouldCopyRedirectHeader(string key)
    {
        return key.EqualsIgnoreCase(HttpHeaderNames.Authorization) == false
               && key.EqualsIgnoreCase(HttpHeaderNames.Cookie) == false;
    }

    protected internal static bool IsRedirectAllowed(HttpRequest request, HttpResponseMessage response, Uri uri)
    {
        if (request.AllowInsecureRedirects)
            return true;

        var sourceUri = response.RequestMessage?.RequestUri ?? request.GetUri();
        return sourceUri.IsAbsoluteUri == false
               || uri.IsAbsoluteUri == false
               || sourceUri.Scheme.EqualsIgnoreCase(Uri.UriSchemeHttps) == false
               || uri.Scheme.EqualsIgnoreCase(Uri.UriSchemeHttp) == false;
    }

    // ReSharper disable once MemberCanBeProtected.Global
    protected internal abstract HttpClientContext CreateHttpClientContext();

    protected override async Task ExecuteAsyncInternal(HttpRequest request, HttpResponse response, CancellationToken token)
    {
        var cts = token.WithTimeout(request.TotalTimeout);
        var context = CreateHttpClientContext();
        var bufferedContent = await CreateBufferedContentAsync(request, cts.Token);
        var responses = new List<HttpResponseMessage>();

        try
        {
            var currentContent = bufferedContent;
            var currentRequest = request;
            var redirectCount = 0;

            while (true)
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace("{Dump}", currentRequest.Dump(this));

                var responseMessage = await SendAsync(context, currentRequest, currentContent, cts.Token);
                responses.Add(responseMessage);

                var responseUri = responseMessage.RequestMessage?.RequestUri;
                response.VisitedUris.AddIfNotNull(responseUri);

                if (request.ReadCookies)
                    ReadCookies(responseMessage, response);

                if (responseMessage.TryGetRedirection(out var uri) == false)
                    break;

                if (IsRedirectAllowed(currentRequest, responseMessage, uri) == false)
                    break;

                if (request.MaxRedirectCount <= 0)
                    break;

                if (response.VisitedUris.Contains(uri))
                    throw new InvalidOperationException("Redirect loop detected.");

                if (redirectCount >= request.MaxRedirectCount)
                    throw new InvalidOperationException($"The maximum number of redirects has been reached: {request.MaxRedirectCount}.");

                redirectCount++;

                currentRequest = CreateRedirectRequest(currentRequest, responseMessage, uri);
                currentContent = responseMessage.StatusCode.PreservesMethodAndContent()
                    ? bufferedContent
                    : null;
            }

            var last = responses.Last(); // responses should not be empty
            response.StatusCode = last.StatusCode;
            ReadHeader(last, response);

            if (request.EnsureSuccessStatusCode)
                last.EnsureSuccess();

            if (request.ReadContent)
            {
                await ReadContentAsync(last, response, cts.Token);

                if (request.ResponseContentType == HttpContentType.Stream)
                {
                    // the last will be disposed in HttpResponse.ResponseStream instead of here.
                    responses.Remove(last);
                }
            }
        }
        finally
        {
            request.Content?.Dispose();
            bufferedContent?.Dispose();

            cts.Dispose();
            responses.ForEach(m => m.Dispose());
            responses.Clear();
            context.Dispose();
        }
    }

}

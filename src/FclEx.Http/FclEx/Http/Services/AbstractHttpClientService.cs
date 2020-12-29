using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public abstract class AbstractHttpClientService : AbstractHttpService
    {
        protected static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

        protected static readonly string[] NotAddHeaderNames =
        {
            HttpKnownHeaderNames.ContentType,
            HttpKnownHeaderNames.Cookie,
            // HttpKnownHeaderNames.UserAgent
        };

        protected AbstractHttpClientService(bool useCookie, IWebProxyExt? proxy = null, ILoggerFactory? loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {
        }

        protected void ReadCookies(HttpResponseMessage response, HttpRes res)
        {
            if (!response.Headers.TryGetValues(HttpKnownHeaderNames.SetCookie, out var cookies)) return;
            var arr = cookies.ToArray();
            if (arr.IsEmpty())
                return;

            res.Headers.AddRange(HttpKnownHeaderNames.SetCookie, arr);
            SaveCookies(response.RequestMessage?.RequestUri!, arr);
        }

        protected static void ReadHeader(HttpResponseMessage response, HttpRes res)
        {
            foreach (var (key, values) in response.Headers.Where(m => m.Key != HttpKnownHeaderNames.SetCookie))
            {
                res.Headers.AddRange(key, values);
            }
        }

        protected static async Task ReadContentAsync(HttpResponseMessage response, HttpRes res, CancellationToken token)
        {
            foreach (var (key, value) in response.Content.Headers)
            {
                res.Headers.AddRange(key, value);
            }
            var bytes = await CopyToMemoryAsync(response.Content, token, res.Req.Timeout).DonotCapture();
            res.ResponseBytes = bytes;

            var req = res.Req;
            switch (req.ResultType)
            {
                case HttpResultType.Byte:
                {
                    res.ResponseBytes = bytes;
                    break;
                }
                case HttpResultType.String:
                {
                    var buffer = new ArraySegment<byte>(bytes.ToArray());
                    (res.ResponseString, res.Encoding) = ReadBufferAsString(buffer, response.Content.Headers, req.CharSet, req.DetectCharSetFromHtmlMeta, req.FallbackCharSet);
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        internal static Encoding? GetEncodingFromCharSet(string? charset)
        {
            if (charset.IsNullOrEmpty())
                return null;

            try
            {
                // Remove at most a single set of quotes.
                if (charset!.Length > 2 &&
                    charset[0] == '\"' &&
                    charset[charset.Length - 1] == '\"')
                {
                    return Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
                }
                else
                {
                    return Encoding.GetEncoding(charset);
                }
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException("The character set provided in ContentType is invalid", e);
            }
        }

        internal static (string, Encoding) ReadBufferAsString(ArraySegment<byte> buffer, HttpContentHeaders headers, string? charSet, bool detectCharSetFromHtmlMeta, string? defaultCharSet)
        {
            Debug.Assert(buffer.Array != null);

            // We don't validate the Content-Encoding header: If the content was encoded, it's the caller's
            // responsibility to make sure to only call ReadAsString() on already decoded content. E.g. if the
            // Content-Encoding is 'gzip' the user should set HttpClientHandler.AutomaticDecompression to get a
            // decoded response stream.

            Encoding? encoding = null;
            var bomLength = -1;

            charSet = (charSet, headers.ContentType?.CharSet).FirstValid();
            // If we do have encoding information in the 'Content-Type' header, use that information to convert
            // the content to a string.
            if (charSet != null)
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

                    if (detectCharSetFromHtmlMeta)
                    {
                        var media = headers.ContentType?.MediaType;
                        if (media != null && media.Contains("html")) // html or xhtml
                        {
                            encoding = DetectCharSetFromHtmlMeta(buffer);
                        }
                    }
                }
            }

            if (encoding == null)
            {
                // Use the default encoding (UTF8) if we couldn't detect one.
                encoding = GetEncodingFromCharSet(defaultCharSet) ?? DefaultStringEncoding;
            }

            // Drop the BOM when decoding the data.
            var str = encoding.GetString(buffer.Array, buffer.Offset + bomLength, buffer.Count - bomLength);
            return (str, encoding);
        }

        private static Encoding? DetectCharSetFromHtmlMeta(ArraySegment<byte> buffer)
        {
            var data = buffer.Array ?? throw new ArgumentNullException(nameof(buffer.Array));
            if (data.Length == 0)
                return null;

            var prefix = Encoding.Default.GetString(data, 0, Math.Min(1024, data.Length));
            var charSet = HtmlUtil.GetMetaCharSet(prefix);
            return charSet == null ? null : Encoding.GetEncoding(charSet);
        }

        private static async Task<byte[]> CopyToMemoryAsync(HttpContent content, CancellationToken token, TimeSpan? timeout)
        {
            var len = content.Headers.ContentLength ?? 0;
            await using var ms = new MemoryStream((int)len);
            await using (var stream = await content.ReadAsStreamAsync(token).DonotCapture())
                await stream.CopyToAsync(ms, token, timeout);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }

        protected static HttpRequestMessage GetHttpRequest(HttpReq req, CookieContainer cc, CancellationToken token)
        {
            var request = new HttpRequestMessage(new HttpMethod(req.Method.ToString().ToUpper()), req.GetUri());
            if (req.Method != HttpMethodType.Get)
            {
                var bytes = req.GetData();
                request.Content = new ArraySegmentContent(bytes, token, req.Timeout)
                {
                    Headers = { ContentType = MediaTypeHeaderValue.Parse(req.ContentType) }
                };
            }

            foreach (var (key, value) in req.HeaderMap.Where(h => !NotAddHeaderNames.Contains(h.Key)))
            {
                request.Headers.Add(key, value);
            }

            var cookies = req.HeaderMap.Get(HttpKnownHeaderNames.Cookie);
            if (!cookies.IsNullOrEmpty())
            {
                request.Headers.Add(HttpKnownHeaderNames.Cookie, cookies);
            }

            var cookiesInCc = cc.GetCookieHeader(request.RequestUri!);
            request.Headers.Add(HttpKnownHeaderNames.Cookie, cookiesInCc);

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpReq httpReq, CancellationToken token,
            HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            var httpRequest = GetHttpRequest(httpReq, _cookieContainer, token);
            var res = await httpClient.SendAsync(httpRequest, httpCompletionOption, token).DonotCapture();
            return res;
        }

        protected async Task ExecuteAsyncInternal(HttpClient httpClient, HttpReq httpReq, HttpRes httpRes, CancellationToken token = default)
        {
            var cts = token.WithTimeout(httpReq.TotalTimeout);
            token = cts.Token;
            var responses = new List<HttpResponseMessage>();
            try
            {
                var curReq = httpReq;
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    var res = await SendAsync(httpClient, curReq, token).DonotCapture();
                    responses.Add(res);
                    httpRes.RedirectUris.Add(res.RequestMessage?.RequestUri!);
                    if (httpReq.ReadResultCookie)
                        ReadCookies(res, httpRes);

                    if (!res.TryGetRedirection(out var uri))
                        break;

                    curReq = HttpReq.Get(uri);
                }

                var response = responses.Last(); // responses should not be empty
                httpRes.StatusCode = response.StatusCode;

                if (httpReq.ReadResultHeader)
                    ReadHeader(response, httpRes);

                if (httpReq.ThrowOnNonSuccessCode)
                    response.EnsureSuccess();

                if (httpReq.ReadResultContent)
                    await ReadContentAsync(response, httpRes, token).DonotCapture();
            }
            finally
            {
                cts.Dispose();
                responses.ForEach(m => m?.Dispose());
                responses.Clear();
            }
        }

    }
}

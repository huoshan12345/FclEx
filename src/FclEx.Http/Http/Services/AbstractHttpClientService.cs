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

        protected AbstractHttpClientService(bool useCookie, IWebProxyExt proxy = null, ILoggerFactory loggerFactory = null)
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
            SaveCookies(response.RequestMessage.RequestUri, arr);
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

            switch (res.Req.ResultType)
            {
                case HttpResultType.Byte:
                {
                    res.ResponseBytes = bytes;
                    break;
                }
                case HttpResultType.String:
                {
                    var headers = response.Content.Headers;
                    if (res.Req.ResultCharSet.IsValid())
                    {
                        if (headers.ContentType == null)
                        {
                            headers.ContentType = new MediaTypeHeaderValue(HttpConstants.DefaultGetContentType);
                        }
                        headers.ContentType.CharSet = res.Req.ResultCharSet;
                    }
                    var buffer = new ArraySegment<byte>(bytes.ToArray());
                    (res.ResponseString, res.Encoding) = ReadBufferAsString(buffer, headers);
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        internal static (string, Encoding) ReadBufferAsString(ArraySegment<byte> buffer, HttpContentHeaders headers)
        {
            // We don't validate the Content-Encoding header: If the content was encoded, it's the caller's
            // responsibility to make sure to only call ReadAsString() on already decoded content. E.g. if the
            // Content-Encoding is 'gzip' the user should set HttpClientHandler.AutomaticDecompression to get a
            // decoded response stream.

            Encoding encoding = null;
            var bomLength = -1;
            var charset = headers.ContentType?.CharSet;

            // If we do have encoding information in the 'Content-Type' header, use that information to convert
            // the content to a string.
            if (charset != null)
            {
                try
                {
                    // Remove at most a single set of quotes.
                    if (charset.Length > 2 &&
                        charset[0] == '\"' &&
                        charset[charset.Length - 1] == '\"')
                    {
                        encoding = Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }

                    // Byte-order-mark (BOM) characters may be present even if a charset was specified.
                    bomLength = EncodingHelper.GetPreambleLength(buffer, encoding);
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException("The character set provided in ContentType is invalid", e);
                }
            }

            // If no content encoding is listed in the ContentType HTTP header, or no Content-Type header present,
            // then check for a BOM in the data to figure out the encoding.
            if (encoding == null)
            {
                if (!EncodingHelper.TryDetectEncoding(buffer, out encoding, out bomLength))
                {
                    // Use the default encoding (UTF8) if we couldn't detect one.
                    encoding = DefaultStringEncoding;

                    // We already checked to see if the data had a UTF8 BOM in TryDetectEncoding
                    // and DefaultStringEncoding is UTF8, so the bomLength is 0.
                    bomLength = 0;
                }
            }

            // Drop the BOM when decoding the data.
            var str = encoding.GetString(buffer.Array, buffer.Offset + bomLength, buffer.Count - bomLength);
            return (str, encoding);
        }

        private static async Task<byte[]> CopyToMemoryAsync(HttpContent content, CancellationToken token, TimeSpan? timeout)
        {
            var len = content.Headers.ContentLength ?? 0;
            using var ms = new MemoryStream((int)len);
            using (var stream = await content.ReadAsStreamAsync().DonotCapture())
                await CopyToAsync(stream, ms, token, timeout);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }

        private static async Task CopyToAsync(Stream source, Stream dest, CancellationToken token, TimeSpan? timeout)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(1024 * 1024);
            try
            {
                int bytesCopied;
                do
                {
                    using var cts = CreateCts(token, timeout);
                    bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cts.Token).DonotCapture();
                    await dest.WriteAsync(buffer, 0, bytesCopied, cts.Token).DonotCapture();
                } while (bytesCopied > 0);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        private static CancellationTokenSource CreateCts(CancellationToken token, TimeSpan? timeout)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            if (timeout.HasValue)
            {
                cts.CancelAfter(timeout.Value);
            }
            return cts;
        }

        protected static HttpRequestMessage GetHttpRequest(HttpReq req, CookieContainer cc)
        {
            var request = new HttpRequestMessage(new HttpMethod(req.Method.ToString().ToUpper()), req.GetUrl());
            if (req.Method != HttpMethodType.Get)
            {
                request.Content = new ByteArrayContent(req.GetBinaryData())
                {
                    Headers = { ContentType = MediaTypeHeaderValue.Parse(req.ContentType) }
                };
            }

            foreach (var (key, value) in req.HeaderMap.Where(h => !NotAddHeaderNames.Contains(h.Key)))
            {
                request.Headers.Add(key, value);
            }

            var cookies = req.HeaderMap.GetOr(HttpKnownHeaderNames.Cookie);
            if (!cookies.IsNullOrEmpty())
            {
                request.Headers.Add(HttpKnownHeaderNames.Cookie, cookies);
            }
            if (cc != null)
            {
                var cookiesInCc = cc.GetCookieHeader(request.RequestUri);
                request.Headers.Add(HttpKnownHeaderNames.Cookie, cookiesInCc);
            }

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpReq httpReq, CancellationToken token, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            using var cts = CreateCts(token, httpReq.Timeout);
            var httpRequest = GetHttpRequest(httpReq, _cookieContainer);
            var res = await httpClient.SendAsync(httpRequest, httpCompletionOption, cts.Token).DonotCapture();
            return res;
        }

        protected async Task ExecuteAsyncInternal(HttpClient httpClient, HttpReq httpReq, HttpRes httpRes, CancellationToken token = default)
        {
            var cts = CreateCts(token, httpReq.TotalTimeout);
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
                    httpRes.RedirectUris.Add(res.RequestMessage.RequestUri);
                    if (httpReq.ReadResultCookie)
                        ReadCookies(res, httpRes);

                    if (!res.IsRedirection())
                        break;

                    var uri = res.GetRedirectUri();
                    curReq = HttpReq.Get(uri);
                }

                var response = responses.Last(); // responses should not be empty
                httpRes.StatusCode = response.StatusCode;

                if (httpReq.ReadResultHeader)
                    ReadHeader(response, httpRes);

                if (httpReq.ThrowOnNonSuccessCode)
                    response.EnsureSuccess();

                if (httpReq.ReadResultContent)
                {
                    await ReadContentAsync(response, httpRes, token).DonotCapture();
                }
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

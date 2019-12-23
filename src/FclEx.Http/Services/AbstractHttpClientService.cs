using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public abstract class AbstractHttpClientService : AbstractHttpService
    {
        protected static readonly string[] NotAddHeaderNames =
        {
            HttpKnownHeaderNames.ContentType,
            HttpKnownHeaderNames.Cookie,
            // HttpKnownHeaderNames.UserAgent
        };

        protected AbstractHttpClientService(
            bool useCookie,
            IWebProxyExt proxy = null,
            ILoggerFactory loggerFactory = null)
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
            foreach (var header in response.Headers.Where(m => m.Key != HttpKnownHeaderNames.SetCookie))
            {
                res.Headers.AddRange(header.Key, header.Value);
            }
        }

        protected static async Task ReadContentAsync(HttpResponseMessage response, HttpRes res)
        {
            switch (res.Req.ResultType)
            {
                case HttpResultType.String:
                    res.ResponseString = await response.Content.ReadAsStringAsync().DonotCapture();
                    break;

                case HttpResultType.Byte:
                    res.ResponseBytes = await response.Content.ReadAsByteArrayAsync().DonotCapture();
                    break;
            }
            foreach (var (key, value) in response.Content.Headers)
            {
                res.Headers.AddRange(key, value);
            }
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

            var cookies = req.HeaderMap.GetOrDefault(HttpKnownHeaderNames.Cookie);
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

        protected async Task ExecuteAsyncInternal(HttpClient httpClient, HttpReq httpReq, HttpRes httpRes, CancellationToken token = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            if (httpReq.Timeout.HasValue)
            {
                cts.CancelAfter(httpReq.Timeout.Value);
            }
            token = cts.Token;
            token.ThrowIfCancellationRequested();

            var responses = new List<HttpResponseMessage>();
            try
            {
                var httpRequest = GetHttpRequest(httpReq, _cookieContainer);
                var response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token).DonotCapture();
                responses.Add(response);
                httpRes.RedirectUris.Add(response.RequestMessage.RequestUri);

                if (httpReq.ReadResultCookie)
                    ReadCookies(response, httpRes);

                while (response.IsRedirection())
                {
                    token.ThrowIfCancellationRequested();
                    var uri = response.GetRedirectUri();
                    var req = GetHttpRequest(HttpReq.Get(uri), _cookieContainer);
                    response = await httpClient.SendAsync(req, token).DonotCapture();
                    responses.Add(response);
                    httpRes.RedirectUris.Add(response.RequestMessage.RequestUri);

                    if (httpReq.ReadResultCookie)
                        ReadCookies(response, httpRes);
                }
                httpRes.StatusCode = response.StatusCode;

                if (httpReq.ReadResultHeader)
                    ReadHeader(response, httpRes);

                if (httpReq.ThrowOnNonSuccessCode)
                    response.EnsureSuccess();

                if (httpReq.ReadResultContent)
                {
                    var contentType = response.Content.Headers.ContentType;
                    httpRes.ResponseChartSet = contentType?.CharSet;
                    if (!httpReq.ResultCharSet.IsNullOrEmpty())
                    {
                        if (contentType == null)
                        {
                            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(httpReq.ContentType);
                            contentType = response.Content.Headers.ContentType;
                        }
                        contentType.CharSet = httpReq.ResultCharSet;
                    }
                    await ReadContentAsync(response, httpRes).DonotCapture();
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

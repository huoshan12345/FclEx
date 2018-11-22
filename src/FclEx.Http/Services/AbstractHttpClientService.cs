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
        protected static readonly string[] _notAddHeaderNames =
        {
            HttpConstants.ContentType,
            HttpConstants.Cookie,
            HttpConstants.UserAgent
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
            if (!response.Headers.TryGetValues(HttpConstants.SetCookie, out var cookies)) return;
            var arr = cookies.ToArray();
            if (arr.IsEmpty())
                return;

            res.Headers.AddRange(HttpConstants.SetCookie, arr);
            SaveCookies(response.RequestMessage.RequestUri, arr);
        }

        protected static void ReadHeader(HttpResponseMessage response, HttpRes res)
        {
            foreach (var header in response.Headers.Where(m => m.Key != HttpConstants.SetCookie))
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
            foreach (var header in response.Content.Headers)
            {
                res.Headers.AddRange(header.Key, header.Value);
            }
        }

        protected static HttpRequestMessage GetHttpRequest(HttpReq req, CookieContainer cc)
        {
            var request = new HttpRequestMessage(new HttpMethod(req.Method.ToString().ToUpper()), req.GetUrl())
            {
                Content = new ByteArrayContent(req.GetBinaryData()) { Headers = { ContentType = MediaTypeHeaderValue.Parse(req.ContentType) } }
            };

            foreach (var header in req.HeaderMap.Where(h => !_notAddHeaderNames.Contains(h.Key)))
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var cookies = req.HeaderMap.GetOrDefault(HttpConstants.Cookie) ??
                          cc?.GetCookieHeader(request.RequestUri);
            if (!cookies.IsNullOrEmpty())
            {
                request.Headers.Add(HttpConstants.Cookie, cookies);
            }

            return request;
        }

        protected async ValueTask<HttpRes> ExecuteAsync(
            HttpClient httpClient, 
            HttpReq httpReq, 
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var responses = new List<HttpResponseMessage>();
            try
            {
                if (httpReq.Timeout.HasValue && token.IsDefault())
                {
                    token = new CancellationTokenSource(httpReq.Timeout.Value).Token;
                }
                var responseItem = new HttpRes { Req = httpReq };
                var httpRequest = GetHttpRequest(httpReq, _cookieContainer);
                var response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token).DonotCapture();
                responses.Add(response);
                responseItem.RedirectUris.Add(response.RequestMessage.RequestUri);

                if (httpReq.ReadResultCookie)
                    ReadCookies(response, responseItem);

                while (response.IsRedirect())
                {
                    var uri = response.GetRedirectUri();
                    var req = new HttpRequestMessage(HttpMethod.Get, uri);
                    var cookies = _cookieContainer.GetCookieHeader(uri);
                    if (!cookies.IsNullOrEmpty()) req.Headers.Add(HttpConstants.Cookie, cookies);
                    response = await httpClient.SendAsync(req, token).DonotCapture();
                    responses.Add(response);
                    responseItem.RedirectUris.Add(response.RequestMessage.RequestUri);

                    if (httpReq.ReadResultCookie)
                        ReadCookies(response, responseItem);
                }
                responseItem.StatusCode = response.StatusCode;

                if (httpReq.ReadResultHeader)
                    ReadHeader(response, responseItem);

                if (httpReq.ThrowOnNonSuccessCode)
                    response.EnsureSuccess();

                if (httpReq.ReadResultContent)
                {
                    var contentType = response.Content.Headers.ContentType;
                    responseItem.ResponseChartSet = contentType?.CharSet;
                    if (!httpReq.ResultCharSet.IsNullOrEmpty())
                    {
                        if (contentType == null)
                        {
                            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(httpReq.ContentType);
                            contentType = response.Content.Headers.ContentType;
                        }
                        contentType.CharSet = httpReq.ResultCharSet;
                    }
                    await ReadContentAsync(response, responseItem).DonotCapture();
                }

                return responseItem;
            }
            finally
            {
                responses.ForEach(m => m?.Dispose());
                responses.Clear();
            }
        }

    }
}

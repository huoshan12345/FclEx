using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http.Services
{
    public class HttpWebRequestService : AbstractHttpService
    {
        private static readonly string[] _notAddHeaderNames =
        {
            HttpConstants.ContentType,
            HttpConstants.Cookie,
            HttpConstants.Referrer,
            HttpConstants.Host,
            HttpConstants.UserAgent,
            HttpConstants.ContentLength,
        };

        public HttpWebRequestService(Uri uri, ILoggerFactory loggerFactory = null, bool useCookie = true)
            : this(useCookie, WebProxyExt.Create(uri), loggerFactory) { }

        public HttpWebRequestService(string url, ILoggerFactory loggerFactory = null, bool useCookie = true)
            : this(useCookie, WebProxyExt.Create(url), loggerFactory) { }

        public HttpWebRequestService(
            bool useCookie = true,
            IWebProxyExt proxy = null,
            ILoggerFactory loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {

        }


        private static HttpWebRequest BuildRequest(HttpReq request, IWebProxyExt proxy, CookieContainer cc)
        {
            var req = (HttpWebRequest)WebRequest.Create(request.GetUrl());
            req.AllowAutoRedirect = false;
            req.ContentType = request.ContentType;
            req.Method = request.Method.ToString().ToUpper();
            req.Referer = request.Referrer;
            req.Host = request.Host;
            req.UserAgent = request.UserAgent;

            if (request.Timeout.HasValue)
                req.Timeout = request.Timeout.Value;

            if (proxy.Type != ProxyType.None)
                req.Proxy = proxy;
            else
                req.Proxy = request.UseDefaultProxy ? WebRequest.DefaultWebProxy : null;


            foreach (var header in request.HeaderMap.Where(h => !_notAddHeaderNames.Contains(h.Key)))
            {
                req.Headers.Add(header.Key, header.Value);
            }

            var cookies = request.HeaderMap.GetOrDefault(HttpConstants.Cookie)
                            ?? cc?.GetCookieHeader(req.RequestUri);

            if (!cookies.IsNullOrEmpty())
                req.Headers.Add(HttpConstants.Cookie, cookies);

            return req;
        }

        private void ReadCookies(HttpWebResponse response)
        {
            var cookieStr = response.Headers[HttpConstants.SetCookie];
            if (cookieStr.IsNullOrEmpty()) return;
            SaveCookies(response.ResponseUri, cookieStr);
        }

        private static void ReadHeader(HttpWebResponse response, HttpRes res)
        {
            foreach (var key in response.Headers.AllKeys)
            {
                var headers = response.Headers.GetValues(key);
                res.Headers.AddRange(key, headers);
            }
        }

        private static async Task ReadContent(HttpWebResponse response, HttpRes res)
        {
            res.ResponseChartSet = response.CharacterSet;

            // 不能依赖content-length
            using (var mem = new MemoryStream())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (response.ContentEncoding?.ToLowerInvariant() == "gzip")
                    {
                        using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                            await gZipStream.CopyToAsync(mem).DonotCapture();
                    }
                    else
                    {
                        await stream.CopyToAsync(mem).DonotCapture();
                    }
                }

                switch (res.Req.ResultType)
                {
                    case HttpResultType.String:
                        var charset = res.Req.ResultCharSet.IsNullOrEmpty()
                            ? response.CharacterSet
                            : res.Req.ResultCharSet;

                        var contentEncoding = charset.IsNullOrEmpty()
                            ? Encoding.UTF8
                            : Encoding.GetEncoding(charset); // GetEncoding returns a cached instance with default settings. 

                        mem.Seek(0, SeekOrigin.Begin);
                        using (var sr = new StreamReader(mem, contentEncoding))
                        {
                            res.ResponseString = sr.ReadToEnd();
                        }
                        break;

                    case HttpResultType.Byte:
                        res.ResponseBytes = mem.ToArray();
                        break;
                }
            }
        }

        public override async Task<HttpRes> ExecuteAsync(HttpReq httpReq, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var responses = new List<HttpWebResponse>();

            try
            {
                var webRequest = BuildRequest(httpReq, WebProxy, _cookieContainer);
                var data = httpReq.GetBinaryData();
                if (!data.IsNullOrEmpty())
                {
                    webRequest.ContentLength = data.Length;
                    //using (var stream = await req.GetRequestStreamAsync().ConfigureAwait(false))
                    //{
                    //    await stream.WriteAsync(data, 0, data.Length, token).DonotCapture();
                    //}

                    // use GetRequestStream() instead of GetRequestStreamAsync() to avoid hangs.
                    using (var stream = webRequest.GetRequestStream())
                    {
                        await stream.WriteAsync(data, 0, data.Length, token).DonotCapture();
                    }
                }
                var responseItem = new HttpRes { Req = httpReq };
                var response = await webRequest.GetHttpResponseAsync().DonotCapture();

                responses.Add(response);
                responseItem.RedirectUris.Add(response.ResponseUri);

                if (httpReq.ReadResultCookie)
                    ReadCookies(response);

                while (response.IsRedirection())
                {
                    var uri = response.GetRedirectUri();
                    var tempReq = BuildRequest(HttpReq.Get(uri), WebProxy, _cookieContainer);
                    response = await tempReq.GetHttpResponseAsync().DonotCapture();
                    responses.Add(response);
                    responseItem.RedirectUris.Add(response.ResponseUri);

                    if (httpReq.ReadResultCookie)
                        ReadCookies(response);
                }
                responseItem.StatusCode = response.StatusCode;

                if (httpReq.ReadResultHeader)
                    ReadHeader(response, responseItem);

                if (httpReq.ReadResultContent)
                    await ReadContent(response, responseItem).DonotCapture();

                if (httpReq.ThrowOnNonSuccessCode)
                    response.EnsureSuccess();

                return responseItem;
            }
            finally
            {
                responses.ForEach(m => m?.Dispose());
                responses.Clear();
            }
        }
        
        public void ClearCookies(Uri uri)
        {
            var cookies = GetCookies(uri);
            foreach (Cookie cookie in cookies)
            {
                cookie.Expired = true;
            }
        }

        public void ClearAllCookies()
        {
            if (_cookieContainer == null) return;
            var cookies = GetAllCookies();
            foreach (var cookie in cookies)
            {
                cookie.Expired = true;
            }
        }
    }
}

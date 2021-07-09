using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public sealed class HttpClientService : AbstractHttpClientService
    {
        public static HttpClientService Default { get; } = new(false);

        private volatile HttpClient _httpClient;

        private static HttpClient CreateHttpClient(IWebProxy? proxy)
        {
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.All,
                ConnectTimeout = Timeout.InfiniteTimeSpan,
                MaxConnectionsPerServer = int.MaxValue,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                Proxy = null,
                UseCookies = false,
                UseProxy = false
            };

            if (proxy != null && !WebProxyExt.None.Equals(proxy))
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }

            var httpClient = new HttpClient(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
            httpClient.DefaultRequestHeaders.Add(HttpKnownHeaderNames.UserAgent, HttpConstants.DefaultUserAgent);
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            return httpClient;
        }

        private HttpClient CreateHttpClient() => CreateHttpClient(WebProxy);

        protected override void SetProxy(IWebProxyExt? proxy)
        {
            proxy ??= WebProxyExt.None;
            if (Equals(_webProxy, proxy)) return;
            _webProxy = proxy;
            _httpClient = CreateHttpClient();
        }

        protected override Task ExecuteAsyncInternal(HttpReq httpReq, HttpRes httpRes, CancellationToken token)
        {
            return ExecuteAsyncInternal(_httpClient, httpReq, httpRes, token);
        }

        public HttpClientService(bool useCookie = true, IWebProxyExt? proxy = null, ILoggerFactory? loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {
            _httpClient = CreateHttpClient();
        }

        public override void Dispose()
        {
        }
    }
}

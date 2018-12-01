using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SocksSharp;
using SocksSharp.Proxy;

namespace FclEx.Http.Services
{
    public sealed class HttpClientService : AbstractHttpClientService
    {
        public static TimerLazy<HttpClientService> Default { get; } = new TimerLazy<HttpClientService>(() =>
                new HttpClientService(false, null, null),
                LazyThreadSafetyMode.ExecutionAndPublication,
                TimeSpan.FromMinutes(2));

        private static readonly TimerLazy<HttpClient> _httpClient =
            new TimerLazy<HttpClient>(() => CreateHttpClient(_funcOfHandler()),
            LazyThreadSafetyMode.ExecutionAndPublication,
            TimeSpan.FromMinutes(2));

        private static Func<HttpMessageHandler> _funcOfHandler;

        private static HttpClientHandler CreateDefaultHandler(IWebProxyExt proxy = null)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                MaxConnectionsPerServer = 64,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }
            else
            {
                handler.UseProxy = false;
                handler.Proxy = null;
            }
            return handler;
        }

        private static HttpMessageHandler CreateHandler(IWebProxyExt proxy)
        {
            switch (proxy.Type)
            {
                case ProxyType.None:
                case ProxyType.Http:
                case ProxyType.Https:
                    return CreateDefaultHandler(proxy);

                case ProxyType.Socks5:
                {
                    return new ProxyClientHandler<Socks5>(new ProxySettings
                    {
                        Port = proxy.Port,
                        Host = proxy.Host,
                        Credentials = proxy.Credentials as NetworkCredential
                    });
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(proxy.Type), proxy.Type, null);
            }
        }

        private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler, true);
            httpClient.DefaultRequestHeaders.Add(HttpConstants.UserAgent, HttpConstants.DefaultUserAgent);
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            return httpClient;
        }

        protected override void SetProxy(IWebProxyExt proxy)
        {
            proxy = proxy ?? WebProxyExt.None;
            if (Equals(_webProxy, proxy)) return;
            _webProxy = proxy;
            _httpClient.Recreate();
        }

        public override ValueTask<HttpRes> ExecuteAsync(HttpReq httpReq, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return ExecuteAsync(_httpClient.Value, httpReq, token);
        }

        public HttpClientService(
            bool useCookie = true,
            IWebProxyExt proxy = null,
            ILoggerFactory loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {
            _funcOfHandler = () => CreateHandler(_webProxy);
        }

        public override void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}

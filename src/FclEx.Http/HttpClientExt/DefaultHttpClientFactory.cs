using System;
using System.Net.Http;
using System.Threading;
using FclEx.Utils;

namespace FclEx.Http.HttpClientExt
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        private readonly IHttpMessageHandlerFactory _httpMessageHandlerFactory;

        public DefaultHttpClientFactory(IHttpMessageHandlerFactory httpMessageHandlerFactory)
        {
            _httpMessageHandlerFactory = Check.NotNull(httpMessageHandlerFactory, nameof(httpMessageHandlerFactory));
        }

        public HttpClient CreateClient(HttpClientOptions options)
        {
            Check.NotNull(options, nameof(options));
            var handler = _httpMessageHandlerFactory.CreateHandler(options);
            var client = new HttpClient(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
            foreach (var action in options.HttpClientActions)
                action(client);
            return client;
        }

        public static DefaultHttpClientFactory Default { get; }
            = new DefaultHttpClientFactory(DefaultHttpMessageHandlerFactory.Default);
    }
}

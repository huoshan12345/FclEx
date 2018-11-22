using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.HttpClientExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http.Services
{
    public class HttpClientExtServiceFactory
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public HttpClientExtServiceFactory(
            IHttpClientFactory clientFactory,
            ILoggerFactory loggerFactory)
        {
            _clientFactory = clientFactory;
            _loggerFactory = loggerFactory;
        }

        public HttpClientExtService Create(HttpClientOptions options = null)
        {
            options = options ?? HttpClientOptions.Default;
            return new HttpClientExtService(options, _clientFactory, _loggerFactory);
        }

        public static HttpClientExtServiceFactory Default { get; }
            = new HttpClientExtServiceFactory(DefaultHttpClientFactory.Default,
                NullLoggerFactory.Instance);
    }
}

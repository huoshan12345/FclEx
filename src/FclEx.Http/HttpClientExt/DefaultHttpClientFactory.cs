// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using FclEx.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FclEx.Http.HttpClientExt
{
    internal class DefaultHttpClientFactory : IHttpClientFactory
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
            var client = new HttpClient(handler, disposeHandler: false);
            return client;
        }

        public static DefaultHttpClientFactory Default { get; } 
            = new DefaultHttpClientFactory(DefaultHttpMessageHandlerFactory.Default);
    }
}

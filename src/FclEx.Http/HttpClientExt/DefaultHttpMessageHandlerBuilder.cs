// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FclEx.Http.Proxy;
using FclEx.Http.Utils;
using FclEx.Utils;

namespace FclEx.Http.HttpClientExt
{
    public class DefaultHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        public DefaultHttpMessageHandlerBuilder(IWebProxyExt proxy)
        {
            Check.NotNull(proxy, nameof(proxy));
            PrimaryHandler = Create(proxy);
        }

        protected static HttpMessageHandler Create(IWebProxyExt proxy)
        {
            return HttpHandlerHelper.Create(proxy);
        }

        public override HttpMessageHandler PrimaryHandler { get; }
        public override IList<DelegatingHandler> AdditionalHandlers { get; set; } = new List<DelegatingHandler>();
        public override HttpMessageHandler Build()
        {
            Check.NotNull(PrimaryHandler, nameof(PrimaryHandler));
            return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
        }
    }
}

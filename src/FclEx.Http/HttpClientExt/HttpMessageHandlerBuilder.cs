using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FclEx.Utils;

namespace FclEx.Http.HttpClientExt
{
    /// <summary>
    /// A builder abstraction for configuring <see cref="HttpMessageHandler"/> instances.
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpMessageHandlerBuilder"/> is registered in the service collection as
    /// a transient service. Callers should retrieve a new instance for each <see cref="HttpMessageHandler"/> to
    /// be created. Implementors should expect each instance to be used a single time.
    /// </remarks>
    public abstract class HttpMessageHandlerBuilder
    {
        /// <summary>
        /// Gets or sets the primary <see cref="HttpMessageHandler"/>.
        /// </summary>
        public abstract HttpMessageHandler PrimaryHandler { get; }

        /// <summary>
        /// Gets a list of additional <see cref="DelegatingHandler"/> instances used to configure an
        /// <see cref="HttpClient"/> pipeline.
        /// </summary>
        public abstract IList<DelegatingHandler> AdditionalHandlers { get; set; }

        /// <summary>
        /// Creates an <see cref="HttpMessageHandler"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpMessageHandler"/> built from the <see cref="PrimaryHandler"/> and
        /// <see cref="AdditionalHandlers"/>.
        /// </returns>
        public abstract HttpMessageHandler Build();

        protected internal static HttpMessageHandler CreateHandlerPipeline(
            HttpMessageHandler primaryHandler,
            IList<DelegatingHandler> additionalHandlers)
        {
            // This is similar to https://github.com/aspnet/AspNetWebStack/blob/master/src/System.Net.Http.Formatting/HttpClientFactory.cs#L58
            // but we don't want to take that package as a dependency.

            if (primaryHandler == null)
            {
                throw new ArgumentNullException(nameof(primaryHandler));
            }

            if (additionalHandlers == null)
            {
                throw new ArgumentNullException(nameof(additionalHandlers));
            }

            var next = primaryHandler;
            for (var i = additionalHandlers.Count - 1; i >= 0; i--)
            {
                var handler = additionalHandlers[i];
                if (handler == null)
                {
                    throw new InvalidOperationException($"The '{nameof(additionalHandlers)}' must not be null.");
                }

                // Checking for this allows us to catch cases where someone has tried to re-use a handler. That really won't
                // work the way you want and it can be tricky for callers to figure out.
                if (handler.InnerHandler != null)
                {
                    const string msg = "The '{0}' property must be null. '{1}' instances provided to '{2}' " +
                                       "must not be reused or cached.{3}Handler: '{4}'";
                    var message = msg.Fmt(
                        nameof(DelegatingHandler.InnerHandler),
                        nameof(DelegatingHandler),
                        nameof(HttpMessageHandlerBuilder),
                        Environment.NewLine,
                        handler);
                    throw new InvalidOperationException(message);
                }

                handler.InnerHandler = next;
                next = handler;
            }

            return next;
        }
    }
}

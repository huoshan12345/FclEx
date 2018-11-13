using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace FclEx.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static bool IsRedirect(this HttpResponseMessage response)
        {
            return response.StatusCode.IsRedirect() && response.Headers.Location != null;
        }

        public static Uri GetRedirectUri(this HttpResponseMessage response)
        {
            var uri = response.Headers.Location;
            if (!uri.IsAbsoluteUri)
                uri = new Uri(response.RequestMessage.RequestUri, uri);
            return uri;
        }

        public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new WebException($"call {httpResponse.RequestMessage.RequestUri} return unsuccessful code: " +
                                       $"{httpResponse.StatusCode}/{httpResponse.StatusCode.ToInt()}");
            }
            return httpResponse;
        }
    }
}

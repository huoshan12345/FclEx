using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace FclEx.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static bool TryGetRedirection(this HttpResponseMessage response, [NotNullWhen(true)] out Uri? uri)
        {
            if (response.StatusCode.IsRedirection() && response.Headers.Location is { } u)
            {
                uri = u.IsAbsoluteUri
                    ? u
                    : new Uri(response.RequestMessage?.RequestUri!, u);
                return true;
            }
            else
            {
                uri = null;
                return false;
            }
        }

        public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new WebException($"call {httpResponse.RequestMessage?.RequestUri} return unsuccessful code: " +
                                       $"{httpResponse.StatusCode}/{httpResponse.StatusCode.ToInt()}");
            }
            return httpResponse;
        }
    }
}

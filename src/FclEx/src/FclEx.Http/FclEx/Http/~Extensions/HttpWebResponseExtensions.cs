using System;
using System.Net;
using FclEx.Extensions;

namespace FclEx.Http
{
    public static class HttpWebResponseExtensions
    {
        public static HttpWebResponse EnsureSuccess(this HttpWebResponse httpResponse)
        {
            if (!httpResponse.StatusCode.IsSuccess())
            {
                throw new WebException($"call {httpResponse.ResponseUri} return unsuccessful code: " +
                                       $"{httpResponse.StatusCode}/{httpResponse.StatusCode.ToInt()}");
            }
            return httpResponse;
        }

        public static Uri GetRedirectUri(this HttpWebResponse response)
        {
            var loc = response.Headers[HttpResponseHeader.Location] 
                      ?? throw new ArgumentNullException(HttpResponseHeader.Location.ToString());
            
            var uri = new Uri(loc, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = new Uri(response.ResponseUri, uri);
            return uri;
        }

        public static bool IsRedirection(this HttpWebResponse response)
        {
            return response.StatusCode.IsRedirection()
                   && response.Headers[HttpResponseHeader.Location] != null;
        }
    }
}

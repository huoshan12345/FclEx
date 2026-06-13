namespace FclEx.Http;

/// <summary>
/// Extensions for legacy <see cref="HttpWebResponse"/> APIs.
/// </summary>
public static class HttpWebResponseExtensions
{
    /// <summary>
    /// Throws when the response status code is outside the HTTP success range.
    /// </summary>
    public static HttpWebResponse EnsureSuccess(this HttpWebResponse response)
    {
        response.StatusCode.EnsureSuccess(response.ResponseUri, response.Method);
        return response;
    }

    /// <summary>
    /// Resolves the Location header against the response URI and returns the redirect target.
    /// </summary>
    public static Uri GetRedirectUri(this HttpWebResponse response)
    {
        var loc = response.Headers[HttpResponseHeader.Location] 
                  ?? throw new ArgumentNullException(HttpResponseHeader.Location.ToString());
            
        var uri = new Uri(loc, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
            uri = new Uri(response.ResponseUri, uri);
        return uri;
    }

    /// <summary>
    /// Returns whether the response is a redirection response with a Location header.
    /// </summary>
    public static bool IsRedirection(this HttpWebResponse response)
    {
        return response.StatusCode.IsRedirection()
               && response.Headers[HttpResponseHeader.Location] != null;
    }
}

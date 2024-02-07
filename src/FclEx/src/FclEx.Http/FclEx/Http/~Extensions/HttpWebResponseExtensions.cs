namespace FclEx.Http;

public static class HttpWebResponseExtensions
{
    public static HttpWebResponse EnsureSuccess(this HttpWebResponse response)
    {
        response.StatusCode.EnsureSuccess(response.ResponseUri, response.Method);
        return response;
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
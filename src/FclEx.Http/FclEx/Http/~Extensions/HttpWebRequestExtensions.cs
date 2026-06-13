namespace FclEx.Http;

/// <summary>
/// Extensions for legacy <see cref="HttpWebRequest"/> APIs.
/// </summary>
public static class HttpWebRequestExtensions
{
    /// <summary>
    /// Runs <see cref="GetHttpResponse"/> on the thread pool so <see cref="HttpWebRequest.Timeout"/> remains effective.
    /// </summary>
    public static async Task<HttpWebResponse> GetHttpResponseAsync(this HttpWebRequest request)
    {
        // use GetHttpResponse instead of GetHttpResponseAsync to make timeout valid.
        // see details at https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.timeout(v=vs.110).aspx
        return await Task.Run(request.GetHttpResponse);
    }

    /// <summary>
    /// Gets the HTTP response, including the error response attached to a <see cref="WebException"/> when the server returned one.
    /// </summary>
    public static HttpWebResponse GetHttpResponse(this HttpWebRequest request)
    {
        try
        {
            return (HttpWebResponse)request.GetResponse();
        }
        catch (WebException ex)
        {
            if (ex.Response != null)
                return (HttpWebResponse)ex.Response;
            else throw;
        }
    }
}

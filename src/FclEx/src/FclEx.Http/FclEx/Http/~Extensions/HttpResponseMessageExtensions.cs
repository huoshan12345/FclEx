namespace FclEx.Http;

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

    public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode == false)
        {
            throw new WebException($"call {response.RequestMessage?.RequestUri} return unsuccessful code: " +
                                   $"{response.StatusCode}/{response.StatusCode.ToInt()}");
        }
        return response;
    }
}
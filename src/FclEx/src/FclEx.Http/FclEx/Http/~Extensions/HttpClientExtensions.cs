namespace FclEx.Http;

public delegate void OnHttpFailedCode(HttpResponseMessage response, string content);

public static class HttpClientExtensions
{
    private static readonly FieldInfo _handler = typeof(HttpMessageInvoker).GetRequiredField("_handler");

    public static HttpMessageHandler GetHandler(this HttpClient httpClient)
    {
        return _handler.GetRequiredValue<HttpMessageHandler>(httpClient);
    }

    public static HttpMessageHandler GetPrimaryHandler(this HttpClient httpClient)
    {
        var handler = httpClient.GetHandler();

        var p = handler;
        while (true)
        {
            var next = (p as DelegatingHandler)?.InnerHandler;
            if (next == null)
                return p;

            p = next;
        }
    }

    public static void IgnoreRemoteCertificateValidation(this HttpClient httpClient)
    {
        var handler = httpClient.GetPrimaryHandler();
        if (handler is SocketsHttpHandler socketsHttpHandler)
        {
            socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        }
        if (handler is HttpClientHandler httpClientHandler)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        }
    }

    public static readonly OnHttpFailedCode ThrowOnFailedCode = (response, content) =>
    {
        var error = content.Truncate(100);
        throw new HttpRequestException(error, null, response.StatusCode);
    };

    public static readonly OnHttpFailedCode IgnoreOnFailedCode = (response, content) => { };

    public static async Task<string> SendAsync(this HttpClient httpClient, HttpMethod method, Uri uri,
        Action<HttpRequestMessage>? configure = null, OnHttpFailedCode? onFailedCode = null)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (uri == null) throw new ArgumentNullException(nameof(uri));

        using var request = new HttpRequestMessage(method, uri);
        configure?.Invoke(request);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var content = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode == false)
        {
            onFailedCode ??= ThrowOnFailedCode;
            onFailedCode.Invoke(response, content);
        }
        return content;
    }

    public static Task<string> SendAsync(this HttpClient httpClient, HttpMethod method, string uri,
        Action<HttpRequestMessage>? configure = null, OnHttpFailedCode? onFailedCode = null)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        return httpClient.SendAsync(method, new Uri(uri, UriKind.RelativeOrAbsolute), configure, onFailedCode);
    }


    public static async Task<T?> SendAsync<T>(this HttpClient httpClient, HttpMethod method, Uri uri,
        Action<HttpRequestMessage>? configure = null, OnHttpFailedCode? onFailedCode = null)
    {
        var content = await httpClient.SendAsync(method, uri, configure, onFailedCode);
        var result = content.FromJson<T>();
        return result;
    }

    public static Task<T?> SendAsync<T>(this HttpClient httpClient, HttpMethod method, string uri,
        Action<HttpRequestMessage>? configure = null, OnHttpFailedCode? onFailedCode = null)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        return httpClient.SendAsync<T>(method, new Uri(uri, UriKind.RelativeOrAbsolute), configure, onFailedCode);
    }
}
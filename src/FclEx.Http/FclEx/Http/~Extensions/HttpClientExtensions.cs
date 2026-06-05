namespace FclEx.Http;

public delegate void OnHttpFailedCode(HttpResponseMessage response, string content);

public static class HttpClientExtensions
{
    /// <summary>
    /// Gets the root message handler stored by <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// This method reads a private runtime field inherited from <see cref="HttpMessageInvoker"/>. It is useful
    /// for diagnostics and tests, but may break if runtime internals change and may not be suitable for trimming
    /// or AOT scenarios.
    /// </remarks>
    public static HttpMessageHandler GetHandler(this HttpClient httpClient)
    {
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<HttpMessageHandler>(httpClient);
    }

    /// <summary>
    /// Gets the last non-delegating handler in the <see cref="HttpClient"/> handler chain.
    /// </summary>
    /// <remarks>
    /// This method depends on <see cref="GetHandler(HttpClient)"/> and therefore reads a private runtime field.
    /// It is best suited for diagnostics and tests rather than application control flow.
    /// </remarks>
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
        switch (handler)
        {
            case SocketsHttpHandler socketsHttpHandler:
                socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback = HttpClientHelper.BypassServerCertificateValidation;
                break;
            case HttpClientHandler httpClientHandler:
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHelper.BypassServerCertificateValidation;
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                break;
        }
    }
}

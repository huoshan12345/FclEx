namespace FclEx.Http;

public delegate void OnHttpFailedCode(HttpResponseMessage response, string content);

public static class HttpClientExtensions
{
    public static HttpMessageHandler GetHandler(this HttpClient httpClient)
    {
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<HttpMessageHandler>(httpClient);
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
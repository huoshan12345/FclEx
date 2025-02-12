#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IHttpAction<T> : IAbstractAction<T>
{
    IHttpService HttpService { get; }
    Uri Uri { get; }
    HttpMethod Method { get; }

    Task<OperationResult<T>> HandleResponseAsync(HttpResponse response)
    {
        return IsFailed(response) 
            ? HandleFailed(response)
            : GetResultAsync(response);
    }

    async Task<OperationResult<T>> IAbstractAction<T>.ExecuteActionAsync(CancellationToken token)
    {
        HttpRequest? request = null;
        try
        {
            request = BuildRequest();
            var response = await HttpService.SendAsync(request, token).IgnoreSyncContext();
            if (response.Error)
            {
#if DEBUG
                Dump(request, HttpService);
#endif
                return (response.Exception!, response.Elapsed);
            }
            return await HandleResponseAsync(response).IgnoreSyncContext();
        }
        catch (Exception ex)
        {
#if DEBUG
            Dump(request, HttpService);
#endif
            return ex;
        }
    }

    protected static void Dump(HttpRequest? request, IHttpService service)
    {
        if (request == null)
            return;

        var msg = new StringBuilder(1024);
        msg.AppendLine("Http Dump: ");
        var url = request.GetUri();
        var header = request.GetRequestHeader(service);
        msg.AppendLine("url: " + url);
        msg.AppendLine("header: ");
        msg.Append(header);
        Debug.WriteLine(msg.ToString());
    }

    HttpRequest BuildRequest()
    {
        var request = HttpRequest.Create(Uri, Method)
            .EnsureSuccessStatusCode(false)
            .AcceptCompress();
        ModifyRequest(request);
        return request;
    }

    void ModifyRequest(HttpRequest request) { }

    bool IsFailed(HttpResponse response) => !response.StatusCode.IsSuccess();

    OperationResult<T> HandleFailed(HttpResponse response)
    {
        var code = response.StatusCode;
        var error = $"The response with status code {code.ToString()}/{code.ToInt()} is unsuccessful: "
                    + response.ResponseString.Truncate(256);
        return error;
    }

    Task<OperationResult<T>> GetResultAsync(HttpResponse response) => GetResult(response);

    OperationResult<T> GetResult(HttpResponse response);
}
#endif
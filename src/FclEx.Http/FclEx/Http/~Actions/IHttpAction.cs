#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IHttpAction<T> : IAbstractAction<T>
{
    IHttpService HttpService { get; }
    Uri Uri { get; }
    HttpMethod Method { get; }

    Task<OperationResult<T>> HandleResponseAsync(HttpResponse res)
    {
        return IsFailed(res) 
            ? HandleFailed(res)
            : GetResultAsync(res);
    }

    async Task<OperationResult<T>> IAbstractAction<T>.ExecuteActionAsync(CancellationToken token)
    {
        HttpRequest? req = null;
        try
        {
            req = BuildRequest();
            var res = await HttpService.SendAsync(req, token).IgnoreSyncContext();
            if (res.HasError)
            {
#if DEBUG
                Dump(req, HttpService);
#endif
                return (res.Exception!, res.Elapsed);
            }
            return await HandleResponseAsync(res).IgnoreSyncContext();
        }
        catch (Exception ex)
        {
#if DEBUG
            Dump(req, HttpService);
#endif
            return ex;
        }
    }

    protected static void Dump(HttpRequest? req, IHttpService service)
    {
        if (req == null)
            return;

        var msg = new StringBuilder(1024);
        msg.AppendLine("Http Dump: ");
        var url = req.GetUri();
        var header = req.GetRequestHeader(service);
        msg.AppendLine("url: " + url);
        msg.AppendLine("header: ");
        msg.Append(header);
        Debug.WriteLine(msg.ToString());
    }

    HttpRequest BuildRequest()
    {
        var req = HttpRequest.Create(Uri, Method)
            .EnsureSuccessStatusCode(false)
            .AcceptCompress();
        ModifyRequest(req);
        return req;
    }

    void ModifyRequest(HttpRequest req) { }

    bool IsFailed(HttpResponse res) => !res.StatusCode.IsSuccess();

    OperationResult<T> HandleFailed(HttpResponse res)
    {
        var code = res.StatusCode;
        var error = $"The res with status code {code.ToString()}/{code.ToInt()} is unsuccessful: "
                    + res.ResponseString.Truncate(256);
        return error;
    }

    Task<OperationResult<T>> GetResultAsync(HttpResponse response) => GetResult(response);

    OperationResult<T> GetResult(HttpResponse response);
}
#endif
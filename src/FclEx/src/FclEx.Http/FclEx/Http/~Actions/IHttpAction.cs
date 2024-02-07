using FclEx.Actions;

namespace FclEx.Http;

public interface IHttpAction<T> : IAbstractAction<T>
{
    IHttpService HttpService { get; }
    Uri Uri { get; }
    HttpMethod Method { get; }

    Task<OperateResult<T>> HandleResponseAsync(HttpResponse res)
    {
        if (IsFailed(res))
            return HandleFailed(res);
        return GetResultAsync(res);
    }

    async Task<OperateResult<T>> IAbstractAction<T>.ExecuteAsyncBody(CancellationToken token)
    {
        HttpRequest? req = null;
        try
        {
            req = BuildRequest();
            var res = await HttpService.SendAsync(req, token).IgnoreSyncContext();
            if (res.HasError)
            {
                Dump(Logger, req, HttpService);
                return (res.Exception!, res.Elapsed);
            }
            return await HandleResponseAsync(res).IgnoreSyncContext();
        }
        catch (Exception ex)
        {
            Dump(Logger, req, HttpService);
            return ex;
        }
    }

    private static void Dump(ILogger logger, HttpRequest? req, IHttpService service)
    {
        if (!logger.IsEnabled(LogLevel.Trace) || req == null)
            return;

        // 此处用于生成请求信息，然后用fiddler等工具测试
        var msg = new StringBuilder(1024);
        msg.AppendLine("Http Dump: ");
        var url = req.GetUri();
        var header = req.GetRequestHeader(service);
        msg.AppendLine("url: " + url);
        msg.AppendLine("header: ");
        msg.Append(header);
        logger.LogTrace(msg.ToString());
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

    static Uri GetUri(Type apiType, Type actionType)
    {
        var urlName = actionType.Name.TrimEnd("Action");
        var value = apiType.GetDataMember(urlName)?.GetValue(null);
        var uri = value switch
        {
            Uri u => u,
            string str => new Uri(str, UriKind.RelativeOrAbsolute),
            _ => throw new Exception("Failed to get url for action: " + actionType.Name)
        };
        return uri;
    }

    bool IsFailed(HttpResponse res) => !res.StatusCode.IsSuccess();

    OperateResult<T> HandleFailed(HttpResponse res)
    {
        var code = res.StatusCode;
        var error = $"The res with status code {code.ToString()}/{code.ToInt()} is unsuccessful: "
                    + res.ResponseString.Truncate(256);
        return error;
    }

    Task<OperateResult<T>> GetResultAsync(HttpResponse response) => GetResult(response);

    OperateResult<T> GetResult(HttpResponse response);
}
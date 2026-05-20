namespace FclEx.Http;

public interface IHttpAction<T> : IPipelineAction<T>, IHttpResponseHandler<T>
{
    IHttpService HttpService { get; }
    Uri Uri { get; }
    HttpMethod Method { get; }
    bool EnsureSuccessStatusCode
#if NET6_0_OR_GREATER
        => true;
#else
    { get; }
#endif

#if NET6_0_OR_GREATER
    Task<OperationResult<T>> IPipelineAction<T>.ExecuteCoreAsync(CancellationToken token)
        => DefaultHttpAction.ExecuteCoreAsync(this, token);
#endif

    HttpRequest BuildRequest()
#if NET6_0_OR_GREATER
        => DefaultHttpAction.BuildRequest(this);
#else
    ;
#endif

    void ModifyRequest(HttpRequest request)
#if NET6_0_OR_GREATER
    { }
#else
    ;
#endif

    Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
#if NET6_0_OR_GREATER
        => DefaultHttpAction.GetResponseAsync(this, request, token);
#else
    ;
#endif

    Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultHttpAction.HandleResponseAsync(this, response);
#else
    ;
#endif
}

public static class DefaultHttpAction
{
    public static async Task<OperationResult<T>> ExecuteCoreAsync<T>(IHttpAction<T> action, CancellationToken token)
    {
        var logger = action.HttpService.Logger;
        HttpRequest? request = null;
        try
        {
            request = action.BuildRequest();
            var response = await action.GetResponseAsync(request, token);
            if (response.IsSuccess)
                return await action.HandleResponseAsync(response)
                    .Then(action.GetResultAsync);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                var dump = request.Dump(action.HttpService);
                logger.LogTrace("{Dump}", dump);
            }

            return (response.Exception, response.Elapsed);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Trace) && request is not null)
            {
                var dump = request.Dump(action.HttpService);
                logger.LogTrace(ex, "{Dump}", dump);
            }
            return ex;
        }
    }

    public static HttpRequest BuildRequest<T>(IHttpAction<T> action)
    {
        var request = HttpRequest.Create(action.Uri, action.Method)
            .EnsureSuccessStatusCode(false)
            .AcceptCompress();
        action.ModifyRequest(request);
        return request;
    }

    public static Task<HttpResponse> GetResponseAsync<T>(IHttpAction<T> action, HttpRequest request, CancellationToken token)
    {
        return action.HttpService.SendAsync(request, token);
    }

    public static Task<OperationResult<HttpResponse>> HandleResponseAsync<T>(IHttpAction<T> action, HttpResponse response)
    {
        if (action.EnsureSuccessStatusCode == false || response.StatusCode.IsSuccess())
            return Operation.Success(response, response.Elapsed);

        var code = response.StatusCode;
        var error = $"The response with status code {code}/{code.ToInt()} is unsuccessful: "
                    + response.ResponseString.Truncate(256);
        return Operation.Error<HttpResponse>(error, response.Elapsed);
    }
}

public abstract class HttpAction<T> : PipelineAction<T>, IHttpAction<T>
{
    public abstract IHttpService HttpService { get; }
    public abstract Uri Uri { get; }
    public abstract HttpMethod Method { get; }
    public virtual bool EnsureSuccessStatusCode => true;
    public virtual HttpRequest BuildRequest() => DefaultHttpAction.BuildRequest(this);
    public virtual void ModifyRequest(HttpRequest request) { }
    public virtual Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
        => DefaultHttpAction.GetResponseAsync(this, request, token);
    public virtual Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
        => DefaultHttpAction.HandleResponseAsync(this, response);
    public abstract OperationResult<T> GetResult(HttpResponse response);
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response)
        => DefaultHttpResponseHandler.GetResultAsync(this, response);
    public override Task<OperationResult<T>> ExecuteCoreAsync(CancellationToken token = default)
        => DefaultHttpAction.ExecuteCoreAsync(this, token);
}
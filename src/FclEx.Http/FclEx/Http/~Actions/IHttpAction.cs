namespace FclEx.Http;

public interface IHttpAction<T> : IAbstractAction<T>, IHttpResponseHandler<T>
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
    async Task<OperationResult<T>> IAbstractAction<T>.ExecuteActionAsync(CancellationToken token)
    {
        var logger = HttpService.Logger;
        HttpRequest? request = null;
        try
        {
            request = BuildRequest();
            var response = await HttpService.SendAsync(request, token);
            if (response.IsSuccess)
                return await HandleResponseAsync(response)
                    .Then(GetResultAsync);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                var dump = request.Dump(HttpService);
                logger.LogTrace(dump);
            }

            return (response.Exception, response.Elapsed);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Trace) && request is not null)
            {
                var dump = request.Dump(HttpService);
                logger.LogTrace(ex, dump);
            }
            return ex;
        }
    }


    HttpRequest BuildRequest()
    {
        var request = HttpRequest.Create(Uri, Method)
            .EnsureSuccessStatusCode(false)
            .AcceptCompress();
        ModifyRequest(request);
        return request;
    }

    void ModifyRequest(HttpRequest request){ }


    Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
    {
        // response.IsError is false here.
        if (EnsureSuccessStatusCode || response.StatusCode.IsSuccess())
            return Operation.Success(response, response.Elapsed);

        var code = response.StatusCode;
        var error = $"The response with status code {code}/{code.ToInt()} is unsuccessful: "
                    + response.ResponseString.Truncate(256);
        return Operation.Error<HttpResponse>(error, response.Elapsed);
    }

    Task<OperationResult<T>> GetResultAsync(HttpResponse response)=> GetResult(response);

#endif
}
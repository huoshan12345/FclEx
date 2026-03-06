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
        var logger = HttpService.Logger;
        HttpRequest? request = null;
        try
        {
            request = BuildRequest();
            var response = await HttpService.SendAsync(request, token);
            if (response.IsError == false)
                return await HandleResponseAsync(response);

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
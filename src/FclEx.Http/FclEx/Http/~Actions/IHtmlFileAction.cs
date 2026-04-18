#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IHtmlFileAction<T> : IHttpAction<T>
{
    string FilePath { get; }

    async Task<OperationResult<T>> IAbstractAction<T>.ExecuteActionAsync(CancellationToken token)
    {
        var logger = HttpService.Logger;
        HttpRequest? request = null;
        try
        {
            request = BuildRequest();
            var text = await File.ReadAllTextAsync(FilePath, token);

            var response = new HttpResponse(request);
            PropertyInfos.HttpResponse_ResponseString.SetValue(response, text);
            PropertyInfos.HttpResponse_StatusCode.SetValue(response, HttpStatusCode.OK);

            return await this.CastTo<IHttpAction<T>>()
                .HandleResponseAsync(response)
                .Then(GetResultAsync);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Trace) && request is not null)
            {
                var dump = request.Dump(HttpService);
                logger.LogTrace(ex, "{Dump}", dump);
            }
            return ex;
        }
    }
}
#endif
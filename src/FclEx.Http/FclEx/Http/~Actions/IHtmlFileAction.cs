namespace FclEx.Http;

public interface IHtmlFileAction<T> : IHttpAction<T>
{
    string FilePath { get; }

#if NET6_0_OR_GREATER
    Task<OperationResult<T>> IPipelineAction<T>.ExecuteActionAsync(CancellationToken token)
        => DefaultHtmlFileAction.ExecuteActionAsync(this, token);
#endif
}

public static class DefaultHtmlFileAction
{
    public static async Task<OperationResult<T>> ExecuteActionAsync<T>(IHtmlFileAction<T> action, CancellationToken token)
    {
        var logger = action.HttpService.Logger;
        HttpRequest? request = null;
        try
        {
            request = action.BuildRequest();
            var text = await File.ReadAllTextAsync(action.FilePath, token);

            var response = new HttpResponse(request);
            PropertyInfos.HttpResponse_ResponseString.SetValue(response, text);
            PropertyInfos.HttpResponse_StatusCode.SetValue(response, HttpStatusCode.OK);

            return await action.CastTo<IHttpAction<T>>()
                .HandleResponseAsync(response)
                .Then(action.GetResultAsync);
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
}

public abstract class HtmlFileAction<T> : HttpAction<T>, IHtmlFileAction<T>
{
    public abstract string FilePath { get; }
    public override Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default)
        => DefaultHtmlFileAction.ExecuteActionAsync(this, token);
}
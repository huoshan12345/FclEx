namespace FclEx.Http;

public static class DefaultHtmlFileAction
{
    public static Uri GetUri<T>(IHtmlFileAction<T> action) => new(Path.GetFullPath(action.FilePath));

    public static async Task<HttpResponse> GetResponseAsync<T>(IHtmlFileAction<T> action, HttpRequest request, CancellationToken token)
    {
        var text = await File.ReadAllTextAsync(action.FilePath, token);

        var response = new HttpResponse(request);
        PropertyInfos.HttpResponse_ResponseString.SetValue(response, text);
        PropertyInfos.HttpResponse_StatusCode.SetValue(response, HttpStatusCode.OK);
        return response;
    }
}
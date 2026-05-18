namespace FclEx.Http;

/// <summary>
/// Provides default behavior for <see cref="IHtmlFileAction{T}"/>.
/// </summary>
public static class DefaultHtmlFileAction
{
    /// <summary>
    /// Converts the action file path to an absolute file URI.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The HTML file action.</param>
    /// <returns>An absolute URI for <see cref="IHtmlFileAction{T}.FilePath"/>.</returns>
    public static Uri GetUri<T>(IHtmlFileAction<T> action) => new(Path.GetFullPath(action.FilePath));

    /// <summary>
    /// Reads the configured file and creates an HTTP-like response.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The HTML file action.</param>
    /// <param name="request">The request associated with the file response.</param>
    /// <param name="token">The cancellation token for file reading.</param>
    /// <returns>A response with status <see cref="HttpStatusCode.OK"/> and the file content as text.</returns>
    public static async Task<HttpResponse> GetResponseAsync<T>(IHtmlFileAction<T> action, HttpRequest request, CancellationToken token)
    {
        var text = await File.ReadAllTextAsync(action.FilePath, token);

        var response = new HttpResponse(request);
        PropertyInfos.HttpResponse_ResponseString.SetValue(response, text);
        PropertyInfos.HttpResponse_StatusCode.SetValue(response, HttpStatusCode.OK);
        return response;
    }
}

namespace FclEx.Http;

/// <summary>
/// Sends a prepared <see cref="HttpRequest"/> as an action.
/// </summary>
public readonly struct HttpRequestAction : IAction<HttpResponse>
{
    private readonly HttpRequest _request;
    private readonly IHttpService _httpService;
    private readonly bool _unwrapError;

    /// <summary>
    /// Initializes a new action for sending an HTTP request.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="httpService">The service used to send the request.</param>
    /// <param name="unwrapError">Whether a failed response should become an error result that still carries the response object.</param>
    public HttpRequestAction(HttpRequest request, IHttpService httpService, bool unwrapError = true)
    {
        _request = request;
        _httpService = httpService;
        _unwrapError = unwrapError;
    }

    /// <summary>
    /// Sends the request and returns the response result.
    /// </summary>
    /// <param name="token">The cancellation token passed to the HTTP service.</param>
    /// <returns>
    /// A successful result for normal responses. If the response has an exception and <c>unwrapError</c> is enabled,
    /// returns an object error containing the response and preserving elapsed time.
    /// </returns>
    public async Task<OperationResult<HttpResponse>> ExecuteAsync(CancellationToken token = default)
    {
        var response = await _httpService.SendAsync(_request, token);
        return response.IsError && _unwrapError
            ? Operation.ObjectError(response, response.Exception!, response.Elapsed).Cast<HttpResponse>()
            : Operation.Success(response, response.Elapsed);
    }
}

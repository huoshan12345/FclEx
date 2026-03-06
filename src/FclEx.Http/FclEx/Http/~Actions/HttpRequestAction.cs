namespace FclEx.Http;

public readonly struct HttpRequestAction : IAction<HttpResponse>
{
    private readonly HttpRequest _request;
    private readonly IHttpService _httpService;
    private readonly bool _unwrapError;

    public HttpRequestAction(HttpRequest request, IHttpService httpService, bool unwrapError = true)
    {
        _request = request;
        _httpService = httpService;
        _unwrapError = unwrapError;
    }

    public async Task<OperationResult<HttpResponse>> ExecuteAsync(CancellationToken token = default)
    {
        var response = await _httpService.SendAsync(_request, token);
        return response.IsError && _unwrapError
            ? Operation.ObjectError(response, response.Exception!, response.Elapsed).Cast<HttpResponse>()
            : Operation.Success(response);
    }
}
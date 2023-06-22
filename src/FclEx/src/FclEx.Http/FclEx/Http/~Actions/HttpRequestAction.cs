using FclEx.Actions;

namespace FclEx.Http;

public readonly struct HttpRequestAction : IAction<HttpResponse>
{
    private readonly HttpRequest _req;
    private readonly IHttpService _httpService;
    private readonly bool _unwrapError;

    public HttpRequestAction(HttpRequest req, IHttpService httpService, bool unwrapError = true)
    {
        _req = req;
        _httpService = httpService;
        _unwrapError = unwrapError;
    }

    public async Task<OperateResult<HttpResponse>> ExecuteAsync(CancellationToken token = default)
    {
        var res = await _httpService.SendAsync(_req, token).DonotCapture();
        return res.HasError && _unwrapError
            ? Operate.CreateObjectError(res, res.Exception!, res.Elapsed).ToExplicit<HttpResponse>()
            : Operate.CreateSuccess(res);
    }
}
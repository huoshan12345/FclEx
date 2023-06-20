namespace FclEx.Actions;

public readonly struct HttpReqAction : IAction<HttpResponse>
{
    private readonly HttpRequest _req;
    private readonly IHttpService _httpService;
    private readonly bool _unwrapError;

    public HttpReqAction(HttpRequest req, IHttpService httpService, bool unwrapError = true)
    {
        _req = req;
        _httpService = httpService;
        _unwrapError = unwrapError;
    }

    public async Task<OperateResult<HttpResponse>> ExecuteAsync(CancellationToken token = default)
    {
        var res = await _httpService.ExecuteAsync(_req, token).DonotCapture();
        return (res.HasError && _unwrapError)
            ? Operate.CreateObjError(res, res.Exception!, res.ExecuteTime).ToExplicit<HttpResponse>()
            : Operate.CreateSuccess(res);
    }
}
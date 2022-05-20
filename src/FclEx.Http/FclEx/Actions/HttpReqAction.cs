using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct HttpReqAction : IAction<HttpRes>
    {
        private readonly HttpReq _req;
        private readonly IHttpService _httpService;
        private readonly bool _unwrapError;

        public HttpReqAction(HttpReq req, IHttpService httpService, bool unwrapError = true)
        {
            _req = req;
            _httpService = httpService;
            _unwrapError = unwrapError;
        }

        public async Task<OperateResult<HttpRes>> ExecuteAsync(CancellationToken token = default)
        {
            var res = await _httpService.ExecuteAsync(_req, token).DonotCapture();
            return (res.HasError && _unwrapError)
                ? Operate.CreateObjError(res, res.Exception!, res.ExcuteTime).ToExplicit<HttpRes>()
                : Operate.CreateSuccess(res);
        }
    }
}

using System;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public class HttpGetAction : AbstractHttpAction
    {
        private readonly Uri _url;
        private readonly HttpResultType _resultType;

        public HttpGetAction(string url, IHttpService httpService, HttpResultType resultType = HttpResultType.String)
            : this(new Uri(url), httpService, resultType)
        {
        }

        public HttpGetAction(Uri url, IHttpService httpService, HttpResultType resultType = HttpResultType.String)
            : base(httpService)
        {
            _url = url;
            _resultType = resultType;
        }

        protected override HttpReq BuildRequest()
        {
            var req = HttpReq.Get(_url)
                .Compress()
                .ResultType(_resultType);
            return req;
        }

        protected override Task<IOperateResult> HandleResponse(HttpRes response)
        {
            var r = _resultType switch
            {
                HttpResultType.String => OperateResult.CreateSuccess(response.ResponseString),
                HttpResultType.Byte => OperateResult.CreateSuccess(response.ResponseBytes),
                _ => (IOperateResult)OperateResult.CreateError("Unknown result type: " + _resultType),
            };
            return Task.FromResult(r);
        }
    }
}

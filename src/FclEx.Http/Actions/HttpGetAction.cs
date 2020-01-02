using System;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http.Actions
{
    public class HttpGetAction : AbstractHttpAction
    {
        private readonly Uri _url;
        private readonly HttpResultType _resultType;

        public HttpGetAction(Uri url, IHttpService httpService, HttpResultType resultType = HttpResultType.String, ILogger logger = null)
            : base(httpService)
        {
            _url = url;
            _resultType = resultType;
            Logger = logger;
        }

        public static HttpGetAction GetString(string url, IHttpService httpService, ILogger logger = null)
            => GetString(new Uri(url), httpService, logger);

        public static HttpGetAction GetString(Uri url, IHttpService httpService, ILogger logger = null)
        {
            return new HttpGetAction(url, httpService, HttpResultType.String, logger);
        }

        public static HttpGetAction GetBytes(string url, IHttpService httpService, ILogger logger = null)
            => GetBytes(new Uri(url), httpService, logger);

        public static HttpGetAction GetBytes(Uri url, IHttpService httpService, ILogger logger = null)
        {
            return new HttpGetAction(url, httpService, HttpResultType.Byte, logger);
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

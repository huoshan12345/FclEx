using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Actions
{
    public interface IHttpAction<T> : IAbstractAction<T>
    {
        IHttpService HttpService { get; }
        Uri Uri { get; }
        HttpReqType ReqType { get; }

        Task<OperateResult<T>> HandleResponseAsync(HttpRes res)
        {
            if (IsFailed(res))
                return HandleFailed(res);
            return GetResultAsync(res);
        }

        async Task<OperateResult<T>> IAbstractAction<T>.ExecuteAsyncBody(CancellationToken token)
        {
            HttpReq? req = null;
            try
            {
                req = BuildRequest();
                var res = await HttpService.ExecuteAsync(req, token).DonotCapture();
                if (res.HasError)
                {
                    Dump(Logger, req, HttpService);
                    return (res.Exception!, res.ExcuteTime);
                }
                return await HandleResponseAsync(res).DonotCapture();
            }
            catch (Exception ex)
            {
                Dump(Logger, req, HttpService);
                return ex;
            }
        }

        private static void Dump(ILogger logger, HttpReq? req, IHttpService service)
        {
            if (!logger.IsEnabled(LogLevel.Trace) || req == null)
                return;

            // 此处用于生成请求信息，然后用fiddler等工具测试
            var msg = new StringBuilder(1024);
            msg.AppendLine("Http Dump: ");
            var url = req.GetUri();
            var header = req.GetRequestHeader(service);
            msg.AppendLine("url: " + url);
            msg.AppendLine("header: ");
            msg.Append(header);
            logger.LogTrace(msg.ToString());
        }

        HttpReq BuildRequest()
        {
            var req = HttpReq.Create(Uri, ReqType)
                .ThrowOnFailedCode(false)
                .Compress();
            ModifyRequest(req);
            return req;
        }

        void ModifyRequest(HttpReq req) { }

        static Uri GetUri(Type apiType, Type actionType)
        {
            var urlName = actionType.Name.TrimEnd("Action");
            var value = apiType.GetDataMember(urlName)?.GetValue(null);
            var uri = value switch
            {
                Uri u => u,
                string str => new Uri(str, UriKind.RelativeOrAbsolute),
                _ => throw new Exception("Failed to get url for action: " + actionType.Name)
            };
            return uri;
        }

        bool IsFailed(HttpRes res) => !res.StatusCode.IsSuccess();

        OperateResult<T> HandleFailed(HttpRes res)
        {
            var code = res.StatusCode;
            var error = $"The res with status code {code.ToString()}/{code.ToInt()} is unsuccessful: "
                        + res.ResponseString.TruncateSafely(256);
            return error;
        }

        Task<OperateResult<T>> GetResultAsync(HttpRes response) => GetResult(response);

        OperateResult<T> GetResult(HttpRes response);
    }
}

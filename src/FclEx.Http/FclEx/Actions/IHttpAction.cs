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
        bool IgnoreFailedStatus { get; }
        bool IgnoreEmptyResponse { get; }

        Task<OperateResult<T>> HandleResponseAsync(HttpRes response)
        {
            var (hasError, error) = GetResponseError(response);
            if (hasError)
                return OperateResult.CreateError<T>(error);
            return GetResultAsync(response);
        }

        async Task<OperateResult<T>> IAbstractAction<T>.ExecuteAsyncBody(CancellationToken token)
        {
            HttpReq? req = null;
            try
            {
                req = BuildRequest();
                var response = await HttpService.ExecuteAsync(req, token).DonotCapture();
                if (response.HasError)
                    return (response.Exception!, response.ExcuteTime);
                return await HandleResponseAsync(response).DonotCapture();
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled(LogLevel.Trace) && req != null)
                {
                    // 此处用于生成请求信息，然后用fiddler等工具测试
                    var msg = new StringBuilder(1024);
                    msg.AppendLine($"[{GetName()}]Http Dump: ");
                    var url = req.GetUri();
                    var header = req.GetRequestHeader(HttpService);
                    msg.AppendLine("url: " + url);
                    msg.AppendLine("header: ");
                    msg.Append(header);
                    Logger.LogTrace(msg.ToString());
                }
                return ex;
            }
        }

        HttpReq BuildRequest()
        {
            var req = HttpReq.Create(Uri, ReqType)
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

        StringError GetResponseError(HttpRes response)
        {
            if (!response.StatusCode.IsSuccess() && !IgnoreFailedStatus)
            {
                var error = $"The response with status code {response.StatusCode} is unsuccessful: "
                         + response.ResponseString.TruncateSafely(256);
                return (true, error);
            }
            if (response.ResponseString.IsNullOrEmpty() && !IgnoreEmptyResponse)
            {
                var error = "The response string is empty";
                return (true, error);
            }
            return (false, "");
        }

        Task<OperateResult<T>> GetResultAsync(HttpRes response) => GetResult(response);
        OperateResult<T> GetResult(HttpRes response);
    }
}

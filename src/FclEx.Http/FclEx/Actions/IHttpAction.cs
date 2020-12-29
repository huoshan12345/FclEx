using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Actions
{
    public interface IHttpAction<T> : IAbstractAction<T>
    {
        IHttpService HttpService { get; set; }

        Task<OperateResult<T>> HandleResponseAsync(HttpRes response);

        sealed async Task<OperateResult<T>> ExecuteInternalAsyncBody(CancellationToken token)
        {
            HttpReq? req = null;
            try
            {
                req = BuildRequest();
                var response = await HttpService.ExecuteAsync(req, token).DonotCapture();
                if (!HandleResponseOnError && response.HasError)
                    return (response.Exception!, response.ExcuteTime);
                var result = await HandleResponseAsync(response).DonotCapture();
                return result.WithElapsed(response.ExcuteTime);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch
            {
                if (Logger.IsEnabled(LogLevel.Trace) && req != null)
                {
                    // 此处用于生成请求信息，然后用fiddler等工具测试
                    var msg = new StringBuilder(1024);
                    msg.AppendLine($"[Action={GetType().ShortName()}]Http Dump: ");
                    var url = req.GetUri();
                    var header = req.GetRequestHeader(HttpService.GetCookies(url));
                    msg.AppendLine("url: " + url);
                    msg.AppendLine("header: ");
                    msg.Append(header);
                    Logger.LogTrace(msg.ToString());
                }
                throw;
            }
        }

        Task<OperateResult<T>> IAbstractAction<T>.ExecuteInternalAsync(CancellationToken token) => ExecuteInternalAsyncBody(token);

        bool HandleResponseOnError { get; }

        Uri Uri { get; }

        HttpReqType ReqType { get; }

        HttpReq BuildRequest()
        {
            var req = HttpReq.Create(Uri, ReqType)
                .Compress();
            ModifyRequest(req);
            return req;
        }

        void ModifyRequest(HttpReq req) { }

        sealed Uri GetUri(Type apiType)
        {
            var type = GetType();
            var urlName = type.Name.TrimEnd("Action");
            var value = apiType.GetDataMember(urlName)?.GetValue(null);
            var uri = value switch
            {
                Uri u => u,
                string str => new Uri(str, UriKind.RelativeOrAbsolute),
                _ => throw new Exception("Failed to get url for action: " + type.Name)
            };
            return uri;
        }
    }
}

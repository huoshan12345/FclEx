using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Actions
{
    public abstract class AbstractHttpAction<T> : AbstractAction<T>
    {
        protected IHttpService HttpService { get; set; }

        protected AbstractHttpAction(IHttpService httpService)
        {
            HttpService = httpService;
        }

        protected abstract Task<IOperateResult<T>> HandleResponseAsync(HttpRes response);

        protected virtual void PreCheckResponse(HttpRes response)
        {
            if (response.HasError)
                response.Exception!.ReThrow();
            else
                response.EnsureSuccessStatusCode();
        }

        protected override async Task<IOperateResult<T>> ExecuteInternalAsync(CancellationToken token = default)
        {
            HttpReq? req = null;
            try
            {
                req = BuildRequest();
                var response = await HttpService.ExecuteAsync(req, token).DonotCapture();
                PreCheckResponse(response);
                var result = await HandleResponseAsync(response).DonotCapture();
                return result;
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
                    msg.AppendLine($"[Action={ActionName}, Http Dump: ");
                    var url = req.GetUrl();
                    var header = req.GetRequestHeader(HttpService.GetCookies(req.Uri));
                    msg.AppendLine("url: " + url);
                    msg.AppendLine("header: ");
                    msg.Append(header);
                    Logger.LogTrace(msg.ToString());
                }
                throw;
            }
        }

        protected abstract string Url { get; }

        protected abstract HttpReqType ReqType { get; }

        protected virtual HttpReq BuildRequest()
        {
            var req = HttpReq.Create(Url, ReqType)
                .Compress();
            ModifyRequest(req);
            return req;
        }

        protected virtual void ModifyRequest(HttpReq req) { }

        protected string GetUrl(ConcurrentDictionary<Type, string> apiDic, Type apiType)
        {
            var actionType = GetType();
            return apiDic.GetOrAdd(actionType, key =>
            {
                var urlName = key.Name.Replace("Action", "");
                var value = apiType.GetTypeInfo().GetField(urlName)?.GetValue(null);
                if (value == null) throw new Exception("Failed to get url by name: " + key.Name);
                return value.ToString();
            });
        }
    }

    public abstract class AbstractHttpAction : AbstractHttpAction<Unit>
    {
        protected AbstractHttpAction(IHttpService httpService) : base(httpService)
        {
        }
    }
}

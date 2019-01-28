using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Event;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public abstract class AbstractHttpAction : AbstractAction
    {
        protected IHttpService HttpService { get; set; }

        protected AbstractHttpAction(
            IHttpService httpService,
            ILogger logger = null,
            ActionEventListener listener = null)
            : base(logger, listener)
        {
            HttpService = httpService;
        }

        protected abstract HttpReq BuildRequest();

        protected abstract Task<ActionEvent> HandleResponse(HttpRes response);

        protected virtual void PreCheckResponse(HttpRes response)
        {
            response.EnsureSuccessStatusCode();
        }

        protected override async Task<ActionEvent> ExecuteInternalAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return await NotifyCancelEventAsync().DonotCapture();

            HttpReq req = null;
            try
            {
                req = BuildRequest();
                var response = await HttpService.ExecuteAsync(req, token).DonotCapture();
                PreCheckResponse(response);
                var result = await HandleResponse(response).DonotCapture();
                return result;
            }
            catch (TaskCanceledException)
            {
                return await NotifyCancelEventAsync().DonotCapture();
            }
            catch (Exception ex)
            {
                var result = await HandleExceptionAsync(ObjectException.Create(req, ex.Message, ex))
                    .DonotCapture();

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

                return result;
            }
            finally
            {
                ++ExcuteTimes;
            }
        }
    }
}

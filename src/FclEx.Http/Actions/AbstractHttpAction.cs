using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public abstract class AbstractHttpAction : AbstractAction
    {
        protected IHttpService HttpService { get; set; }

        protected AbstractHttpAction(IHttpService httpService)
        {
            HttpService = httpService;
        }

        protected abstract HttpReq BuildRequest();

        protected abstract Task<IOperateResult> HandleResponse(HttpRes response);

        protected virtual void PreCheckResponse(HttpRes response)
        {
            if (response.HasError)
                response.Exception.ReThrow();
            else
                response.EnsureSuccessStatusCode();
        }

        protected override async Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default)
        {
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
    }
}

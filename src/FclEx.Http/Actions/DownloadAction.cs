using System;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Event;
using FclEx.Http.Services;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public class DownloadAction : AbstractHttpAction
    {
        protected readonly string _url;

        public DownloadAction(string url, 
            IHttpService httpService,
            ILogger logger = null,
            ActionEventListener listener = null)
            : base(httpService, logger, listener)
        {
            _url = url;
        }

        protected override HttpReq BuildRequest()
        {
            var req = HttpReq.Get(_url);
            req.Compress();
            req.ResultType = HttpResultType.Byte;
            ModifyRequest(req);
            return req;
        }

        protected virtual void ModifyRequest(HttpReq req) { }

        protected override Task<ActionEvent> HandleResponse(HttpRes response)
        {
            return NotifyOkEventAsync(response.ResponseBytes);
        }
    }
}

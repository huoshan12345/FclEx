using System;
using System.IO;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Event;
using FclEx.Http.Services;

namespace FclEx.Http.Actions
{
    public class DownloadFileAction : DownloadAction
    {
        protected readonly string _filePath;

        public DownloadFileAction(string url, string filePath, IHttpService httpService) : base(url, httpService)
        {
            _filePath = filePath;
        }

        public DownloadFileAction(Uri url, string filePath, IHttpService httpService) : this(url.ToString(), filePath, httpService)
        {
        }

        protected override Task<ActionEvent> HandleResponse(HttpRes response)
        {
            try
            {
                File.WriteAllBytes(_filePath, response.ResponseBytes);
                return NotifyOkEventAsync(_filePath);
            }
            catch (Exception ex)
            {
                return NotifyErrorAsync(ex);
            }
        }
    }
}

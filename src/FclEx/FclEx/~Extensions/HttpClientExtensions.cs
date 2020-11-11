using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FclEx
{
    public static class HttpClientExtensions
    {
        public static async Task DownLoadFileAsync(this HttpClient httpClient, string url, string savePath)
        {
            var response = await httpClient.GetAsync(url).DonotCapture();
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync().DonotCapture();
            if (File.Exists(savePath)) return;
            using (var file = File.Create(savePath))
            {
                await stream.CopyToAsync(file).DonotCapture();
            }
        }

        public static Task DownLoadFileAsync(this HttpClient httpClient, string url, string saveDir, string fileName)
        {
            return DownLoadFileAsync(httpClient, url, Path.Combine(saveDir, fileName));
        }
    }
}

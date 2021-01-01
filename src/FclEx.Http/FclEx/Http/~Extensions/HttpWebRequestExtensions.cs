using System.Net;
using System.Threading.Tasks;

namespace FclEx.Http
{
    public static class HttpWebRequestExtensions
    {
        public static async Task<HttpWebResponse> GetHttpResponseAsync(this HttpWebRequest req)
        {
            // use GetHttpResponse instead of GetHttpResponseAsync to make timeout valid.
            // see details at https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.timeout(v=vs.110).aspx
            return await Task.Run(req.GetHttpResponse).DonotCapture();
        }

        public static HttpWebResponse GetHttpResponse(this HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    return (HttpWebResponse)ex.Response;
                else throw;
            }
        }
    }
}

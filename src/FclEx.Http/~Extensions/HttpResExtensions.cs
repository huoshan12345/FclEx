using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Http.Core;
using MimeTypes.Core;
using Newtonsoft.Json.Linq;

namespace FclEx.Http
{
    public static class HttpResExtensions
    {
        public static HttpRes EnsureSuccessStatusCode(this HttpRes res)
        {
            if (res.StatusCode.IsSuccess())
            {
                throw new WebException($"call {res.Req.GetUrl()} with {res.Req.Method} return unsuccessful code: {res.StatusCode}/{res.StatusCode.ToInt()}");
            }
            return res;
        }

        public static HttpRes ThrowIfError(this HttpRes res)
        {
            if (res.HasError) res.Exception.ReThrow();
            return res;
        }

        public static async Task<HttpRes> ThrowIfError(this Task<HttpRes> task)
        {
            var res = await task.DonotCapture();
            res.ThrowIfError();
            return res;
        }

        public static async Task<T> ReadJsonAs<T>(this Task<HttpRes> task)
        {
            var res = await task.DonotCapture();
            res.ThrowIfError();
            if (res.Req.ResultType == HttpResultType.Byte)
                throw new InvalidOperationException("Can not deserialize json from byte array.");
            var resObj = res.ResponseString.ToJToken().ToObject<T>();
            return resObj;
        }

        public static Encoding GetResultEncoding(this HttpRes res)
        {
            return res.ResponseChartSet.IsValid()
                ? Encoding.GetEncoding(res.ResponseChartSet)
                : Encoding.UTF8;
        }

        internal static HttpFileDownloadInfo GetDownloadInfo(this HttpRes res)
        {
            var realUrl = res.RedirectUris.Last();
            var fileNameWithExt = Path.GetFileName(realUrl.LocalPath);
            var ext = Path.GetExtension(fileNameWithExt);
            var fileName = fileNameWithExt.TrimEnd(ext);
            if (fileName.IsNullOrEmpty())
            {
                fileName = (realUrl.Host + realUrl.LocalPath).RegexReplace(@"\W", "_").TrimEnd("_");
            }
            if (ext.IsNullOrEmpty())
            {
                var mimeType = res.Headers.GetFirstOrDefault(HttpKnownHeaderNames.ContentType);
                if (mimeType.IsValid())
                {
                    if (mimeType.Contains(";"))
                    {
                        var contentType = new ContentType(mimeType);
                        mimeType = contentType.MediaType;
                    }
                    if (MimeTypeMap.TryGetExtension(MimeTypeFix(mimeType), out var extension))
                        ext = extension;
                }
            }
            ext ??= string.Empty;

            var bytes = res.Req.ResultType == HttpResultType.Byte
                ? res.ResponseBytes
                : res.GetResultEncoding().GetBytes(res.ResponseString);

            var info = new HttpFileDownloadInfo(realUrl, fileName, ext, bytes);
            return info;
        }

        internal static string MimeTypeFix(string mimeType)
        {
            // avoid throwing exceptions in MimeTypeMap.TryGetExtension.
            switch (mimeType)
            {
                case null: return string.Empty;
                case "image/jpg": return "image/jpeg";
                default: return mimeType.TrimStart(".");
            }
        }
    }


}

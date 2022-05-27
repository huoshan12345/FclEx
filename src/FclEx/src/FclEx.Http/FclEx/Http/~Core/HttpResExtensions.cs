using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Utils;
using MimeTypes.Core;
using Newtonsoft.Json.Linq;

namespace FclEx.Http
{
    public static class HttpResExtensions
    {
        public static HttpRes EnsureSuccessStatusCode(this HttpRes res)
        {
            if (!res.StatusCode.IsSuccess())
            {
                throw new WebException($"call {res.HttpReq.GetUri()} with {res.HttpReq.Method} return unsuccessful code: {res.StatusCode}/{res.StatusCode.ToInt()}");
            }
            return res;
        }

        public static HttpRes ThrowIfError(this HttpRes res)
        {
            if (res.HasError) res.Exception!.ReThrow();
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
            if (res.HttpReq.ResultType == HttpResultType.Bytes)
                throw new InvalidOperationException("Can not deserialize json from byte array.");
            if (res.ResponseString.IsNullOrEmpty())
                throw new InvalidOperationException("Can not deserialize json from empty response string.");
            var resObj = res.ResponseString!.ToJToken().ToObject<T>();
            return resObj!;
        }

        public static OperateResult<T> ReadJson<T>(this HttpRes res, string? path = null)
        {
            var str = res.ResponseString;
            if (!str.IsPossibleJson())
                return Operate.CreateError<T>("Can not parse json from empty string");

            JToken? token = str!.ToJToken();
            if (path.IsValid())
                token = token.SelectToken(path!);

            if (token == null)
                return Operate.CreateError<T>("The path does not exist in json: " + path);

            return token.ToObject<T>()!;
        }

        public static HttpFileDownloadInfo GetDownloadInfo(this HttpRes res)
        {
            var realUrl = res.RedirectUris.Last();
            var fileNameWithExt = Path.GetFileName(realUrl.LocalPath);
            var ext = Path.GetExtension(fileNameWithExt);
            var fileName = fileNameWithExt.TrimEnd(ext);
            if (fileName.IsNullOrEmpty())
            {
                fileName = (realUrl.Host + realUrl.LocalPath).RegexReplace(@"\W", "_").TrimEnd("_");
            }

            var mimeType = res.Headers.GetFirstOr(HttpKnownHeaderNames.ContentType) ?? "";
            if (mimeType.IsValid())
            {
                if (mimeType!.Contains(";"))
                {
                    var contentType = new ContentType(mimeType);
                    mimeType = contentType.MediaType;
                }
                if (MimeTypeMap.TryGetExtension(MimeTypeFix(mimeType), out var extension))
                    ext = extension;
            }

            ext ??= string.Empty;
            var info = new HttpFileDownloadInfo(realUrl, fileName, ext, res.ResponseBytes, mimeType);
            return info;
        }

        internal static string MimeTypeFix(string mimeType)
        {
            // avoid throwing exceptions in MimeTypeMap.TryGetExtension.
            return mimeType switch
            {
                null => string.Empty,
                "image/jpg" => "image/jpeg",
                _ => mimeType.TrimStart(".")
            };
        }

        public static Uri LastUri(this HttpRes res) => res.RedirectUris.Last();

        public static Task<HttpRes> Error(this Task<HttpRes> task, Action<Exception> action)
        {
            return task.On(m => m.HasError, m => action(m.Exception!));
        }

        public static Task<HttpRes> Ok(this Task<HttpRes> task, Action<HttpRes> action)
        {
            return task.On(m => !m.HasError, action);
        }

        public static Task<HttpRes> Ok(this Task<HttpRes> task, Func<HttpRes, Task> action)
        {
            return task.On(m => !m.HasError, action);
        }
    }


}

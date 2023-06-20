using System.Net.Mime;
using System.Text.RegularExpressions;
using MimeTypes.Core;
using Newtonsoft.Json.Linq;

namespace FclEx.Http;

public static class HttpResponseExtensions
{
    public static HttpResponse EnsureSuccessStatusCode(this HttpResponse res)
    {
        if (!res.StatusCode.IsSuccess())
        {
            throw new WebException($"call {res.Request.GetUri()} with {res.Request.Method} return unsuccessful code: {res.StatusCode}/{res.StatusCode.ToInt()}");
        }
        return res;
    }

    public static HttpResponse ThrowIfError(this HttpResponse res)
    {
        if (res.HasError) res.Exception!.ReThrow();
        return res;
    }

    public static async Task<HttpResponse> ThrowIfError(this Task<HttpResponse> task)
    {
        var res = await task.DonotCapture();
        res.ThrowIfError();
        return res;
    }

    public static async Task<T> ReadJsonAs<T>(this Task<HttpResponse> task)
    {
        var res = await task.DonotCapture();
        res.ThrowIfError();
        if (res.Request.ResponseType == HttpResponseType.Bytes)
            throw new InvalidOperationException("Can not deserialize json from byte array.");
        if (res.ResponseString.IsNullOrEmpty())
            throw new InvalidOperationException("Can not deserialize json from empty response string.");
        var resObj = res.ResponseString!.ToJToken().ToObject<T>();
        return resObj!;
    }

    public static OperateResult<T> ReadJson<T>(this HttpResponse res, string? path = null)
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


    private static readonly Regex _regexOfNonWord = new(@"\W", RegexOptions.Compiled);
    public static HttpFileDownloadInfo GetDownloadInfo(this HttpResponse res)
    {
        var realUrl = res.RedirectUris.Last();
        var fileNameWithExt = Path.GetFileName(realUrl.LocalPath);
        var ext = Path.GetExtension(fileNameWithExt);
        var fileName = fileNameWithExt.TrimEnd(ext);
        if (fileName.IsNullOrEmpty())
        {
            fileName = (realUrl.Host + realUrl.LocalPath).Replace(_regexOfNonWord, "_").TrimEnd("_");
        }

        var mimeType = res.Headers.GetFirstOr(HttpKnownHeaderNames.ContentType) ?? "";
        if (mimeType.IsValid())
        {
            if (mimeType.Contains(';'))
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

    public static Uri LastUri(this HttpResponse res) => res.RedirectUris.Last();

    public static Task<HttpResponse> Error(this Task<HttpResponse> task, Action<Exception> action)
    {
        return task.Do(m => m.HasError, m => action(m.Exception!));
    }

    public static Task<HttpResponse> Ok(this Task<HttpResponse> task, Action<HttpResponse> action)
    {
        return task.Do(m => !m.HasError, action);
    }

    public static Task<HttpResponse> Ok(this Task<HttpResponse> task, Func<HttpResponse, Task> action)
    {
        return task.Do(m => !m.HasError, action);
    }
}
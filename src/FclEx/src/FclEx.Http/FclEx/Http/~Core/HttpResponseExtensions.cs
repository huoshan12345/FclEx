using MimeTypes;

namespace FclEx.Http;

public static class HttpResponseExtensions
{
    public static HttpResponse EnsureSuccessStatusCode(this HttpResponse response)
    {
        var request = response.Request;
        response.StatusCode.EnsureSuccess(request.GetUri(), request.Method.Method);
        return response;
    }

    public static HttpResponse ThrowIfError(this HttpResponse response)
    {
        if (response.HasError)
            response.Exception.ReThrow();
        return response;
    }

    public static Task<HttpResponse> ThrowIfError(this Task<HttpResponse> task)
    {
        return task.Then(m => m.ThrowIfError());
    }

    public static Task<T> ReadJsonAsRequired<T>(this Task<HttpResponse> task, string? path = null)
    {
        return task.Then(m => m.ReadJsonAs<T>(path)).GetRequiredValue();
    }

    public static Task<OperationResult<T>> ReadJsonAs<T>(this Task<HttpResponse> task, string? path = null)
    {
        return task.Then(m => m.ReadJsonAs<T>(path));
    }

    public static OperationResult<T> ReadJsonAs<T>(this HttpResponse response, string? path = null, JsonSerializerOptions? options = null)
    {
        if (response.Exception is { } ex)
            return (ex, response.Elapsed);

        var str = response.ResponseString;
        if (!str.IsPossibleJson())
            return Operation.CreateError<T>("Can not parse json from empty string");

        var doc = JsonDocument.Parse(str, new()
        {
            AllowTrailingCommas = options?.AllowTrailingCommas ?? false,
            CommentHandling = options?.ReadCommentHandling ?? JsonCommentHandling.Disallow,
            MaxDepth = options?.MaxDepth ?? 0,
        });

        var element = path.IsNullOrEmpty()
            ? doc.RootElement
            : doc.SelectElement(path);

        return element.HasValue
            ? element.Value.Deserialize<T>(options)!
            : Operation.CreateError<T>("The path does not exist in json: " + path);
    }

    private static readonly Regex _regexOfNonWord = new(@"\W", RegexOptions.Compiled);
    public static HttpFileDownloadInfo GetDownloadInfo(this HttpResponse response)
    {
        var realUrl = response.RedirectUris.Last();
        var fileNameWithExt = Path.GetFileName(realUrl.LocalPath);
        var ext = Path.GetExtension(fileNameWithExt);
        var fileName = fileNameWithExt.TrimEnd(ext);
        if (fileName.IsNullOrEmpty())
        {
            fileName = (realUrl.Host + realUrl.LocalPath).Replace(_regexOfNonWord, "_").TrimEnd("_");
        }

        var mimeType = response.Headers.GetLast(HttpKnownHeaderNames.ContentType) ?? "";
        if (mimeType.IsNotEmpty())
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
        var info = new HttpFileDownloadInfo(realUrl, fileName, ext, response.ResponseBytes, mimeType);
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
        return task.When(m => m.HasError, m => action(m.Exception!));
    }

    public static Task<HttpResponse> Ok(this Task<HttpResponse> task, Action<HttpResponse> action)
    {
        return task.When(m => !m.HasError, action);
    }

    public static Task<HttpResponse> Ok(this Task<HttpResponse> task, Func<HttpResponse, Task> action)
    {
        return task.When(m => !m.HasError, action);
    }

    public static HttpResponse AddCookies(this HttpResponse response, IEnumerable<string> cookies)
    {
        response.Headers.AddRange(HttpKnownHeaderNames.SetCookie, cookies);
        return response;
    }
}
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
        if (response.IsError)
            response.Exception.ReThrow();
        return response;
    }

    public static Task<HttpResponse> ThrowIfError(this Task<HttpResponse> task)
    {
        return task.Then(m => m.ThrowIfError());
    }

    public static Task<T> ReadJsonAsRequired<T>(this Task<HttpResponse> task, string? path = null)
    {
        return task.Then(m => m.ReadJsonAs<T>(path)).Unwrap();
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
            return Operation.Error<T>("Can not parse json from empty string");

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
            : Operation.Error<T>("The path does not exist in json: " + path);
    }
    
    public static async Task<OperationResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, DownloadOptions options)
    {
        var request = new HttpRequest(options.Uri, options.Method)
            .ReadAsBytes()
            .ReadHeadersTimeout(options.ConnectTimeout)
            .ReadBufferTimeout(options.ReadBufferTimeout)
            .AcceptCompress();

        if (options.Content is { } content)
        {
            request.Content(content);
        }

        var response = await request.SendAsync(http, options.CancellationToken);
        return response.IsError
            ? Operation.ObjectError(response, response.Exception, response.Elapsed)
                .Cast<HttpFileDownloadInfo>()
            : response.GetDownloadInfo(options.FileBaseName, options.FileExtension);
    }

    private static readonly Regex _regexOfNonWord = new(@"\W+", RegexOptions.Compiled);

    public static HttpFileDownloadInfo GetDownloadInfo(this HttpResponse response, string? baseName = null, string? extension = null)
    {
        var uri = response.VisitedUris.Last();
        var fileName = uri.Segments
            .Select(m => m.Trim('/'))
            .LastOrDefault(m => m.IsNotEmpty());
        var ext = Path.GetExtension(fileName);

        if (baseName is null)
        {
            baseName = fileName.TrimEnd(ext);
            if (baseName.IsNullOrEmpty())
            {
                baseName = uri.Host.Replace(_regexOfNonWord, "_").TrimEnd("_");
            }
        }

        var mimeType = response.Headers.GetLast(HttpHeaderNames.ContentType) ?? "";

        if (extension is null)
        {
            if (mimeType.IsNotEmpty())
            {
                if (mimeType.Contains(';'))
                {
                    var contentType = new ContentType(mimeType);
                    mimeType = contentType.MediaType;
                }
                if (MimeTypeMap.TryGetExtension(MimeTypeFix(mimeType), out extension))
                    ext = extension;
            }
        }

        ext ??= string.Empty;
        var info = new HttpFileDownloadInfo(uri, baseName, ext, response.ResponseBytes, mimeType);
        return info;

        static string MimeTypeFix(string mimeType)
        {
            // avoid throwing exceptions in MimeTypeMap.TryGetExtension.
            return mimeType switch
            {
                null => string.Empty,
                "image/jpg" => "image/jpeg",
                _ => mimeType.TrimStart("."),
            };
        }
    }

    public static Uri LastUri(this HttpResponse response) => response.VisitedUris.Last();

    public static Task<HttpResponse> Error(this Task<HttpResponse> task, Action<Exception> action)
    {
        return task.When(m => m.IsError, m => action(m.Exception!));
    }

    public static Task<HttpResponse> Ok(this Task<HttpResponse> task, Action<HttpResponse> action)
    {
        return task.When(m => !m.IsError, action);
    }

    public static Task<HttpResponse> Ok(this Task<HttpResponse> task, Func<HttpResponse, Task> action)
    {
        return task.When(m => !m.IsError, action);
    }

    public static HttpResponse AddCookies(this HttpResponse response, IEnumerable<string> cookies)
    {
        response.Headers.AddRange(HttpHeaderNames.SetCookie, cookies);
        return response;
    }

    public static bool TryGetMediaType(this HttpResponse response, [NotNullWhen(true)] out MediaTypeHeaderValue? mediaType)
    {
        if (response.Headers.TryGetValue(HttpHeaderNames.ContentType, out var contentTypes))
        {
            foreach (var m in contentTypes.Reverse().NotNull())
            {
                if (MediaTypeHeaderValue.TryParse(m, out mediaType))
                {
                    return true;
                }
            }
        }

        mediaType = null;
        return false;
    }
}
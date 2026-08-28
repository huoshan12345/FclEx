using MimeTypes;

namespace FclEx.Http;

/// <summary>
/// Helpers for validating, parsing, and extracting data from <see cref="HttpResponse"/>.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Throws when the response status code is outside the HTTP success range.
    /// The exception message includes the request URI and HTTP method.
    /// </summary>
    public static HttpResponse EnsureSuccessStatusCode(this HttpResponse response)
    {
        var request = response.Request;
        response.StatusCode.EnsureSuccess(request.GetUri(), request.Method.Method);
        return response;
    }

    /// <summary>
    /// Rethrows the captured response exception when <see cref="HttpResponse.IsError"/> is true.
    /// </summary>
    public static HttpResponse ThrowIfError(this HttpResponse response)
    {
        if (response.IsError)
            response.Exception.ReThrow();
        return response;
    }

    /// <summary>
    /// Awaits a response task and rethrows the captured response exception when present.
    /// </summary>
    public static Task<HttpResponse> ThrowIfError(this Task<HttpResponse> task)
    {
        return task.Then(m => m.ThrowIfError());
    }

    /// <summary>
    /// Awaits a response task, reads JSON from the response string, and unwraps the operation result.
    /// </summary>
    /// <remarks>An error result is converted to an exception by <c>Unwrap</c>.</remarks>
    public static Task<T> ReadJsonAsRequired<T>(this Task<HttpResponse> task, string? path = null)
    {
        return task.Then(m => m.ReadJsonAs<T>(path)).Unwrap();
    }

    /// <summary>
    /// Awaits a response task and reads JSON from the response string.
    /// </summary>
    public static Task<OperationResult<T>> ReadJsonAs<T>(this Task<HttpResponse> task, string? path = null)
    {
        return task.Then(m => m.ReadJsonAs<T>(path));
    }

    /// <summary>
    /// Reads the response string as JSON and deserializes either the root element or the element selected by <paramref name="path"/>.
    /// Captured response exceptions are returned as operation errors.
    /// </summary>
    public static OperationResult<T> ReadJsonAs<T>(this HttpResponse response, string? path = null, JsonSerializerOptions? options = null)
    {
        if (response.Exception is { } ex)
            return (ex, response.Elapsed);

        var str = response.ResponseString;
        return str.IsPossibleJson()
            ? Operation.Execute(() => Deserialize(str, path, options))
            : Operation.Error<T>("Can not parse json from non-JSON string");

        static T Deserialize(string str, string? path, JsonSerializerOptions? options)
        {
            var root = JsonNode.Parse(str, new JsonNodeOptions
            {
                PropertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive ?? false,
            });
            var node = root.SelectNodes(path).FirstOrDefault()
                       ?? throw new KeyNotFoundException("The path does not exist in json: " + path);

            return node.Deserialize<T>(options)
                ?? throw new InvalidOperationException("The value is null for path: " + path);
        }
    }

    private static readonly Regex _regNonWord = new(@"\W+", RegexOptions.Compiled);

    /// <summary>
    /// Creates download metadata from a byte response.
    /// Non-null <paramref name="baseName"/> or <paramref name="extension"/> values override names derived from the final URI, Content-Type header, or MIME map.
    /// </summary>
    public static HttpFileDownloadInfo GetDownloadInfo(this HttpResponse response, string? baseName = null, string? extension = null)
    {
        var uri = response.LastUri();
        var fileName = uri.Segments
            .Select(m => m.Trim('/'))
            .LastOrDefault(m => m.IsNotEmpty());

        if (fileName.IsNotEmpty())
        {
            var (name, ext) = Path.GetNameAndExtension(fileName);
            baseName ??= name;
            extension ??= ext;
        }

        if (baseName.IsNullOrEmpty())
        {
            baseName = uri.Host.Replace(_regNonWord, "_").TrimEnd('_');
        }

        var mimeType = response.Headers.GetLast(HttpHeaderNames.ContentType) ?? "";

        if (extension.IsNullOrEmpty() && mimeType.IsNotEmpty())
        {
            if (mimeType.Contains(';'))
            {
                var contentType = new ContentType(mimeType);
                mimeType = contentType.MediaType;
            }

            if (MimeTypeMap.TryGetExtension(MimeTypeFix(mimeType), out var e))
                extension = e;
        }

        extension ??= "";
        var info = new HttpFileDownloadInfo(uri, baseName, extension, response.ResponseBytes, mimeType);
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

    /// <summary>
    /// Returns the last URI visited by the response workflow.
    /// Throws when the response has no visited URI record.
    /// </summary>
    public static Uri LastUri(this HttpResponse response)
    {
        return response.VisitedUris.LastOrDefault() ?? throw new InvalidOperationException("No visited URIs available.");
    }

    /// <summary>
    /// Adds raw Set-Cookie header values to the response header collection.
    /// </summary>
    public static HttpResponse AddCookies(this HttpResponse response, IEnumerable<string> cookies)
    {
        response.Headers.AddRange(HttpHeaderNames.SetCookie, cookies);
        return response;
    }

    /// <summary>
    /// Attempts to parse the last valid Content-Type header value as a media type.
    /// Header values are checked from newest to oldest.
    /// </summary>
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

    /// <summary>
    /// Creates a redirect action when a redirect URL can be resolved from a response.
    /// </summary>
    /// <param name="response">The response used to resolve the redirect URL.</param>
    /// <param name="httpService">The service used to send the redirect request.</param>
    /// <param name="urlFunc">Returns the redirect URL. Returning <see langword="null"/> means no redirect action is created.</param>
    /// <returns>A GET redirect action, or <see langword="null"/> when no URL is available.</returns>
    public static IAction<HttpResponse>? TryCreateRedirectAction(this HttpResponse response, IHttpService httpService, Func<HttpResponse, string?> urlFunc)
    {
        Check.NotNull(urlFunc);
        var url = urlFunc(response);
        return url == null ? null : HttpRequest.Get(url).ToAction(httpService);
    }

    /// <summary>
    /// Creates a redirect action when the given URL is not null.
    /// </summary>
    /// <param name="response">The response associated with the redirect decision.</param>
    /// <param name="httpService">The service used to send the redirect request.</param>
    /// <param name="url">The redirect URL. When <see langword="null"/>, no action is created.</param>
    /// <returns>A GET redirect action, or <see langword="null"/> when <paramref name="url"/> is null.</returns>
    public static IAction<HttpResponse>? TryCreateRedirectAction(this HttpResponse response, IHttpService httpService, string? url)
    {
        return response.TryCreateRedirectAction(httpService, r => url);
    }
}

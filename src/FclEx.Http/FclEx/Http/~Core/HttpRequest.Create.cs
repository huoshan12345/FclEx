namespace FclEx.Http;

partial class HttpRequest
{
    /// <summary>
    /// Creates a request model from an absolute or relative URI and an HTTP method.
    /// </summary>
    /// <param name="uri">Initial request URI.</param>
    /// <param name="method">HTTP method used when sending the request.</param>
    public static HttpRequest Create(Uri uri, HttpMethod method) => new(uri, method);

    /// <summary>
    /// Creates a request model from a URI string and an HTTP method.
    /// The URI string may be absolute or relative.
    /// </summary>
    /// <param name="uri">Initial request URI string.</param>
    /// <param name="method">HTTP method used when sending the request.</param>
    public static HttpRequest Create(string uri, HttpMethod method) => Create(new Uri(uri, UriKind.RelativeOrAbsolute), method);

    /// <summary>
    /// Creates a request model from an HTTP method and a URI string.
    /// The URI string may be absolute or relative.
    /// </summary>
    /// <param name="method">HTTP method used when sending the request.</param>
    /// <param name="uri">Initial request URI string.</param>
    public static HttpRequest Create(HttpMethod method, string uri) => Create(uri, method);

    /// <summary>
    /// Creates a GET request model for the specified URI.
    /// </summary>
    public static HttpRequest Get(Uri uri) => Create(uri, HttpMethod.Get);

    /// <summary>
    /// Creates a POST request model for the specified URI.
    /// </summary>
    public static HttpRequest Post(Uri uri) => Create(uri, HttpMethod.Post);

    /// <summary>
    /// Creates a PUT request model for the specified URI.
    /// </summary>
    public static HttpRequest Put(Uri uri) => Create(uri, HttpMethod.Put);

    /// <summary>
    /// Creates a DELETE request model for the specified URI.
    /// </summary>
    public static HttpRequest Delete(Uri uri) => Create(uri, HttpMethod.Delete);

    /// <summary>
    /// Creates a HEAD request model for the specified URI.
    /// </summary>
    public static HttpRequest Head(Uri uri) => Create(uri, HttpMethod.Head);

    /// <summary>
    /// Creates an OPTIONS request model for the specified URI.
    /// </summary>
    public static HttpRequest Options(Uri uri) => Create(uri, HttpMethod.Options);

    /// <summary>
    /// Creates a GET request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Get(string uri) => Get(new Uri(uri, UriKind.RelativeOrAbsolute));

    /// <summary>
    /// Creates a POST request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Post(string uri) => Post(new Uri(uri, UriKind.RelativeOrAbsolute));

    /// <summary>
    /// Creates a PUT request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Put(string uri) => Put(new Uri(uri, UriKind.RelativeOrAbsolute));

    /// <summary>
    /// Creates a DELETE request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Delete(string uri) => Delete(new Uri(uri, UriKind.RelativeOrAbsolute));

    /// <summary>
    /// Creates a HEAD request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Head(string uri) => Head(new Uri(uri, UriKind.RelativeOrAbsolute));

    /// <summary>
    /// Creates an OPTIONS request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Options(string uri) => Options(new Uri(uri, UriKind.RelativeOrAbsolute));

#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a CONNECT request model for the specified URI.
    /// </summary>
    public static HttpRequest Connect(Uri uri) => Create(uri, HttpMethod.Connect);

    /// <summary>
    /// Creates a CONNECT request model for an absolute or relative URI string.
    /// </summary>
    public static HttpRequest Connect(string uri) => Connect(new Uri(uri, UriKind.RelativeOrAbsolute));
#endif
}

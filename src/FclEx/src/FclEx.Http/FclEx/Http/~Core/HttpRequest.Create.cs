namespace FclEx.Http;

partial class HttpRequest
{
    public static HttpRequest Create(Uri uri, HttpMethod method) => new(uri, method);
    public static HttpRequest Create(string uri, HttpMethod method) => Create(new Uri(uri, UriKind.RelativeOrAbsolute), method);
    public static HttpRequest Create(HttpMethod method, string uri) => Create(uri, method);

    public static HttpRequest Get(Uri uri) => Create(uri, HttpMethod.Get);
    public static HttpRequest Post(Uri uri) => Create(uri, HttpMethod.Post);
    public static HttpRequest Put(Uri uri) => Create(uri, HttpMethod.Put);
    public static HttpRequest Delete(Uri uri) => Create(uri, HttpMethod.Delete);
    public static HttpRequest Head(Uri uri) => Create(uri, HttpMethod.Head);
    public static HttpRequest Options(Uri uri) => Create(uri, HttpMethod.Options);

    public static HttpRequest Get(string uri) => Get(new Uri(uri, UriKind.RelativeOrAbsolute));
    public static HttpRequest Post(string uri) => Post(new Uri(uri, UriKind.RelativeOrAbsolute));
    public static HttpRequest Put(string uri) => Put(new Uri(uri, UriKind.RelativeOrAbsolute));
    public static HttpRequest Delete(string uri) => Delete(new Uri(uri, UriKind.RelativeOrAbsolute));
    public static HttpRequest Head(string uri) => Head(new Uri(uri, UriKind.RelativeOrAbsolute));
    public static HttpRequest Options(string uri) => Options(new Uri(uri, UriKind.RelativeOrAbsolute));

#if NET7_0_OR_GREATER
    public static HttpRequest Connect(Uri uri) => Create(uri, HttpMethod.Connect);
    public static HttpRequest Connect(string uri) => Connect(new Uri(uri, UriKind.RelativeOrAbsolute));
#endif
}
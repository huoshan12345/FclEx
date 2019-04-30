using FclEx.Extensions;

namespace FclEx.Http.Core
{
    public static class HttpConstants
    {
        public const string Host = "Host";
        public const string Location = "Location";
        public const string UserAgent = "User-Agent";
        public const string Referrer = "Referer";
        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";
        public const string SetCookie = "Set-Cookie";
        public const string Origin = "Origin";
        public const string Cookie = "Cookie";
        public const string DefaultGetContentType = "text/html";
        public const string DefaultPostContentType = FormContentType;
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3236.0 Safari/537.36";
        public const string JsonContentType = "application/json";
        public const string FormContentType = "application/x-www-form-urlencoded";
        public const string MultiPartContentType = "multipart/form-data";
        public const string ByteArrayContentType = "application/octet-stream";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptLanguage = "Accept-Language";
        public const string Accept = "Accept";
        public const string Boundary = "boundary";
        public const string NewLine = "\r\n";
        public static byte[] NewLineBytes { get; } = NewLine.ToUtf8Bytes();
        public static string EncapsulationBoundary { get; } = "--";
        public static byte[] EncapsulationBoundaryBytes { get; } = EncapsulationBoundary.ToUtf8Bytes();
    }
}

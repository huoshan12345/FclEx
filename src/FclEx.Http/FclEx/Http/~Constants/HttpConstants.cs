namespace FclEx.Http;

public static class HttpConstants
{
    public const string DefaultGetContentType = "text/html";
    public const string DefaultPostContentType = FormContentType;
    public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36";
    public const string JsonContentType = "application/json";
    public const string FormContentType = "application/x-www-form-urlencoded";
    public const string MultiPartContentType = "multipart/form-data";
    public const string ByteArrayContentType = "application/octet-stream";

    public const string Boundary = "boundary";
    public const string NewLine = "\r\n";
    public static byte[] NewLineBytes { get; } = NewLine.ToUtf8Bytes();
    public static string EncapsulationBoundary { get; } = "--";
    public static byte[] EncapsulationBoundaryBytes { get; } = EncapsulationBoundary.ToUtf8Bytes();


    internal static readonly string[] CookieDateTimeFormats =
    [
        "ddd, d MMM yyyy HH:mm:ss Z",
        "ddd, d-MMM-yyyy HH:mm:ss Z",
        "ddd, d-MMM-yy HH:mm:ss Z",
    ];
}
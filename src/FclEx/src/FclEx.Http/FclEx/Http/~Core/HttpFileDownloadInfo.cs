using System;

namespace FclEx.Http;

public readonly struct HttpFileDownloadInfo
{
    public HttpFileDownloadInfo(Uri fileUrl, string fileNameWithoutExt, string fileExt, byte[] bytes, string mimeType)
    {
        FileUrl = fileUrl;
        FileExt = fileExt;
        FileNameWithoutExt = fileNameWithoutExt;
        FileName = fileNameWithoutExt + fileExt;
        Bytes = bytes;
        MimeType = mimeType;
    }

    public string FileNameWithoutExt { get; }
    public string FileName { get; }
    public string FileExt { get; }
    public byte[] Bytes { get; }
    public Uri FileUrl { get; }
    public string MimeType { get; }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Http.Core
{
    public readonly struct HttpFileDownloadInfo
    {
        public HttpFileDownloadInfo(Uri fileUrl, string fileNameWithoutExt, string fileExt, byte[] bytes)
        {
            FileUrl = fileUrl;
            FileExt = fileExt;
            FileNameWithoutExt = fileNameWithoutExt;
            FileName = fileNameWithoutExt + fileExt;
            Bytes = bytes;
        }

        public string FileNameWithoutExt { get; }
        public string FileName { get; }
        public string FileExt { get; }
        public byte[] Bytes { get; }
        public Uri FileUrl { get; }
    }
}

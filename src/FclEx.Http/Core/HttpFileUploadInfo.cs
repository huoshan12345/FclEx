namespace FclEx.Http.Core
{
    public readonly struct HttpFileUploadInfo
    {
        public HttpFileUploadInfo(string name, string fileName, string contentType)
        {
            Name = name;
            FileName = fileName;
            ContentType = contentType;
        }

        public string Name { get; }
        public string FileName { get; }
        public string ContentType { get; }
    }
}
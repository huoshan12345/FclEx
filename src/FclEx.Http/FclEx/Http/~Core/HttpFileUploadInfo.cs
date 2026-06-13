namespace FclEx.Http;

/// <summary>
/// Describes a file part in a multipart/form-data upload.
/// </summary>
public readonly struct HttpFileUploadInfo
{
    /// <summary>
    /// Initializes upload metadata for one file part.
    /// </summary>
    /// <param name="name">The multipart form field name.</param>
    /// <param name="fileName">The file name sent in the content disposition header.</param>
    /// <param name="contentType">The content type of the uploaded file content.</param>
    public HttpFileUploadInfo(string name, string fileName, string contentType)
    {
        Name = name;
        FileName = fileName;
        ContentType = contentType;
    }

    /// <summary>
    /// The multipart form field name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The file name sent in the content disposition header.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The content type of the uploaded file content.
    /// </summary>
    public string ContentType { get; }
}

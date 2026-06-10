namespace FclEx.Http;

/// <summary>
/// Represents detailed information about a file being downloaded via HTTP.
/// </summary>
/// <remarks>
/// This class contains metadata and content for a file download, including the file's URL,
/// name (with and without extension), its MIME type, and the raw byte content of the file.
/// </remarks>
public class HttpFileDownloadInfo
{
    public HttpFileDownloadInfo(Uri fileUrl, string fileNameWithoutExtension, string fileExtension, byte[] fileBytes, string mimeType)
    {
        FileUrl = fileUrl ?? throw new ArgumentNullException(nameof(fileUrl));
        FileNameWithoutExtension = fileNameWithoutExtension ?? throw new ArgumentNullException(nameof(fileNameWithoutExtension));
        FileExtension = fileExtension ?? throw new ArgumentNullException(nameof(fileExtension));
        FileBytes = fileBytes ?? throw new ArgumentNullException(nameof(fileBytes));
        MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        FileName = fileNameWithoutExtension + fileExtension;
    }

    /// <summary>
    /// The file's name without the extension (e.g., "document" from "document.pdf").
    /// </summary>
    public string FileNameWithoutExtension { get; }

    /// <summary>
    /// The full file name including the extension (e.g., "document.pdf").
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The file's extension (e.g., ".pdf").
    /// </summary>
    public string FileExtension { get; }

    /// <summary>
    /// The raw byte content of the file being downloaded.
    /// </summary>
    public byte[] FileBytes { get; }

    /// <summary>
    /// The URL from which the file is being downloaded.
    /// </summary>
    public Uri FileUrl { get; }

    /// <summary>
    /// The MIME type of the file (e.g., "application/pdf").
    /// </summary>
    public string MimeType { get; }
}
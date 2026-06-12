namespace FclEx.Http;

/// <summary>
/// Contains the bytes and resolved file metadata returned by an HTTP download.
/// </summary>
/// <remarks>
/// Instances are normally created by <see cref="HttpResponseExtensions.GetDownloadInfo(HttpResponse, string?, string?)"/>
/// or the download helpers. The constructor stores the supplied values as-is, except that
/// <see cref="FileName"/> is computed by concatenating <see cref="FileNameWithoutExtension"/> and
/// <see cref="FileExtension"/>. It does not add a leading dot to the extension, infer a MIME type,
/// sanitize file-system characters, or copy the byte array.
/// </remarks>
public class HttpFileDownloadInfo
{
    /// <summary>
    /// Creates a download result from an already resolved URL, file name parts, content bytes, and MIME type.
    /// </summary>
    /// <param name="fileUrl">The final URL associated with the downloaded bytes, usually the last visited URI after redirects.</param>
    /// <param name="fileNameWithoutExtension">The base file name to expose, without any extension suffix.</param>
    /// <param name="fileExtension">The extension suffix to append to the base name, including the leading dot when one is desired.</param>
    /// <param name="fileBytes">The downloaded content bytes. The array reference is stored directly.</param>
    /// <param name="mimeType">The response MIME type used for this result, or an empty string when none was resolved.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
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
    /// Gets the resolved base file name without <see cref="FileExtension"/>.
    /// </summary>
    public string FileNameWithoutExtension { get; }

    /// <summary>
    /// Gets the full file name formed by concatenating <see cref="FileNameWithoutExtension"/> and <see cref="FileExtension"/>.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the resolved extension suffix. This value may be empty and is not normalized by the constructor.
    /// </summary>
    public string FileExtension { get; }

    /// <summary>
    /// Gets the downloaded content bytes.
    /// </summary>
    public byte[] FileBytes { get; }

    /// <summary>
    /// Gets the URL associated with the downloaded content.
    /// </summary>
    public Uri FileUrl { get; }

    /// <summary>
    /// Gets the resolved MIME type string, or an empty string when no MIME type was available.
    /// </summary>
    public string MimeType { get; }
}

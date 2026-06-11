namespace FclEx.Http.Core;

public class HttpFileDownloadInfoTests
{
    [Fact]
    public void Constructor_SetsFileMetadataAndCombinesFileName()
    {
        var uri = new Uri("https://example.com/files/report.pdf");
        byte[] bytes = [1, 2, 3];

        var info = new HttpFileDownloadInfo(uri, "report", ".pdf", bytes, "application/pdf");

        Assert.Equal(uri, info.FileUrl);
        Assert.Equal("report", info.FileNameWithoutExtension);
        Assert.Equal(".pdf", info.FileExtension);
        Assert.Equal("report.pdf", info.FileName);
        Assert.Same(bytes, info.FileBytes);
        Assert.Equal("application/pdf", info.MimeType);
    }

    [Theory]
    [InlineData("fileUrl")]
    [InlineData("fileNameWithoutExtension")]
    [InlineData("fileExtension")]
    [InlineData("fileBytes")]
    [InlineData("mimeType")]
    public void Constructor_WhenRequiredArgumentIsNull_ThrowsArgumentNullException(string parameterName)
    {
        var uri = new Uri("https://example.com/files/report.pdf");
        byte[] bytes = [1, 2, 3];

        var ex = Assert.Throws<ArgumentNullException>(() => parameterName switch
        {
            "fileUrl" => new HttpFileDownloadInfo(null!, "report", ".pdf", bytes, "application/pdf"),
            "fileNameWithoutExtension" => new HttpFileDownloadInfo(uri, null!, ".pdf", bytes, "application/pdf"),
            "fileExtension" => new HttpFileDownloadInfo(uri, "report", null!, bytes, "application/pdf"),
            "fileBytes" => new HttpFileDownloadInfo(uri, "report", ".pdf", null!, "application/pdf"),
            "mimeType" => new HttpFileDownloadInfo(uri, "report", ".pdf", bytes, null!),
            _ => throw new ArgumentOutOfRangeException(nameof(parameterName), parameterName, null),
        });

        Assert.Equal(parameterName, ex.ParamName);
    }
}

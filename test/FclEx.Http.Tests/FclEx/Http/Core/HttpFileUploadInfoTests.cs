namespace FclEx.Http.Core;

public class HttpFileUploadInfoTests
{
    [Fact]
    public void Constructor_SetsUploadMetadata()
    {
        var info = new HttpFileUploadInfo("avatar", "me.png", "image/png");

        Assert.Equal("avatar", info.Name);
        Assert.Equal("me.png", info.FileName);
        Assert.Equal("image/png", info.ContentType);
    }
}

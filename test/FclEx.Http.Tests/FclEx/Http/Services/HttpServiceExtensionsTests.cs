namespace FclEx.Http.Services;

public class HttpServiceExtensionsTests
{
    [Theory]
    [InlineData("https://www.google.com/", "www_google_com.html")]
    [InlineData("https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns", "covariant-returns.html")]
    [InlineData("https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/#comments", "csharp-exploring-extension-members.html")]
    public async Task DownloadAsync_Test(string uri, string fileName)
    {
        using var http = new HttpClientService();

        var (successful, file, exception, _) = await http.DownloadAsync(uri);

        Assert.True(successful, () => exception!.ToString());
        Assert.NotNull(file);
        Assert.Equal(fileName, file.FileName);
        Assert.Equal(Path.GetExtension(fileName), file.FileExtension);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), file.FileNameWithoutExtension);
    }

    [Fact(Skip = "no proxy")]
    public async Task DownloadAsync_WithProxy_403_Test()
    {
        using var http = HttpClientService.Create(DefaultProxy);

        const string url = "https://scontent-lga3-1.cdninstagram.com/v/t51.2885-15/e35/84633088_233319031038964_4686527252914001142_n.jpg" +
                           "?_nc_ht=scontent-lga3-1.cdninstagram.com&_nc_cat=104&_nc_ohc=rtLj-eg1T_sAX8YuTB5&oh=ee63e1a1e272f0826565ba4dc8f31174&oe=5E4D0FBF";

        var (successful, _, ex, _) = await http.DownloadAsync(url);

        Assert.False(successful, () => ex!.ToString());
        Assert.True(ex.IsObjectException<HttpResponse>(m => m.StatusCode == HttpStatusCode.Forbidden));
    }
}
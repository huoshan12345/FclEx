namespace FclEx.Http.Services;

public class HttpServiceExtensionsTests
{
    [RetryTheory]
    [InlineData("https://www.google.com/", "www_google_com.html")]
    [InlineData("https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns", "covariant-returns.html")]
    [InlineData("https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/#comments", "csharp-exploring-extension-members.html")]
    public async Task DownloadAsync_Test(string uri, string fileName)
    {
        using var http = new HttpClientService();

        var (success, file, exception, _) = await http.DownloadAsync(uri);

        Assert.True(success, () => exception!.ToString());
        Assert.NotNull(file);
        Assert.Equal(fileName, file.FileName);
        Assert.Equal(Path.GetExtension(fileName), file.FileExtension);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), file.FileNameWithoutExtension);
    }
}
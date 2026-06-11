namespace FclEx.Http.Core;

public class DownloadOptionsTests
{
    [Fact]
    public void DownloadOptions_DefaultsToGetAndNoCancellation()
    {
        var options = new DownloadOptions
        {
            Uri = new Uri("https://example.com/file"),
        };

        Assert.Equal(HttpMethod.Get, options.Method);
        Assert.Null(options.Content);
        Assert.Null(options.BufferSize);
        Assert.Null(options.ReadHeadersTimeout);
        Assert.Null(options.ReadBufferTimeout);
        Assert.False(options.CancellationToken.IsCancellationRequested);
        Assert.Null(options.FileBaseName);
        Assert.Null(options.FileExtension);
    }

    [Fact]
    public void BatchDownloadOptions_DefaultsToGetAndParallelExecution()
    {
        var options = new BatchDownloadOptions();

        Assert.Null(options.BaseAddress);
        Assert.Equal(HttpMethod.Get, options.Method);
        Assert.Null(options.Content);
        Assert.Null(options.ReadHeadersTimeout);
        Assert.Null(options.BufferSize);
        Assert.Null(options.ReadBufferTimeout);
        Assert.True(options.ExecuteInParallel);
        Assert.Null(options.Concurrency);
        Assert.False(options.CancellationToken.IsCancellationRequested);
    }
}

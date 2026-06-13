namespace FclEx.Http.HttpContents;

public class BufferedContentTests
{
    [Fact]
    public async Task CreateAsync_BuffersContentAndCopiesHeadersExceptContentLength()
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Json);
        source.Headers.ContentLanguage.Add("en-US");
        source.Headers.ContentLength = 999;

        using var buffered = await BufferedContent.CreateAsync(source);
        source.Dispose();

        var text = await buffered.ReadAsStringAsync();

        Assert.Equal("payload", text);
        Assert.Equal(MediaTypes.Json, buffered.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, buffered.Headers.ContentType?.CharSet);
        Assert.Contains("en-US", buffered.Headers.ContentLanguage);
        Assert.Equal(7, buffered.Headers.ContentLength);
    }

    [Fact]
    public async Task Clone_CreatesIndependentReadableContentWithCopiedHeaders()
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Text);
        using var buffered = await BufferedContent.CreateAsync(source);

        using var clone = buffered.Clone();

        Assert.NotSame(buffered, clone);
        Assert.Equal(MediaTypes.Text, clone.Headers.ContentType?.MediaType);
        Assert.Equal("payload", await clone.ReadAsStringAsync());
        Assert.Equal("payload", await buffered.ReadAsStringAsync());
    }

    [Fact]
    public async Task CloneIfDisposed_WhenContentHasNotBeenDisposed_ReturnsSameInstance()
    {
        using var source = new StringContent("payload");
        using var buffered = await BufferedContent.CreateAsync(source);

        var result = buffered.CloneIfDisposed();

        Assert.Same(buffered, result);
    }

    [Fact]
    public async Task CloneIfDisposed_WhenContentHasBeenDisposed_ReturnsReadableClone()
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Text);
        var buffered = await BufferedContent.CreateAsync(source);

        buffered.Dispose();
        using var clone = buffered.CloneIfDisposed();

        Assert.NotSame(buffered, clone);
        Assert.Equal(MediaTypes.Text, clone.Headers.ContentType?.MediaType);
        Assert.Equal("payload", await clone.ReadAsStringAsync());
    }
}

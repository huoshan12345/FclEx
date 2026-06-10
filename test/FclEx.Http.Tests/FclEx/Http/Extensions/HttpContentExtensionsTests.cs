namespace FclEx.Http.Extensions;

public class HttpContentExtensionsTests
{
    [Fact]
    public async Task ReadAsByteArrayAsync_ReturnsContentBytes()
    {
        using var content = new StringContent("hello", Encoding.UTF8, MediaTypes.Text);

        var bytes = await content.ReadAsByteArrayAsync(1, TimeSpan.FromSeconds(1), CancellationToken.None);

        Assert.Equal("hello", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task ReadAsStreamAsync_WhenContentLengthExceedsInt32MaxValue_ThrowsClearError()
    {
        using var content = new StringContent("");
        content.Headers.ContentLength = (long)int.MaxValue + 1;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            content.ReadAsStreamAsync(null, null, CancellationToken.None));

        Assert.Contains("Content length is too large", ex.Message);
    }

    [Fact]
    public async Task ToBufferedContentAsync_WhenContentIsNull_ReturnsNull()
    {
        HttpContent? content = null;

        var buffered = await content.ToBufferedContentAsync();

        Assert.Null(buffered);
    }

    [Fact]
    public async Task ToBufferedContentAsync_WhenContentIsAlreadyBuffered_ReturnsSameInstance()
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Text);
        using var buffered = await source.ToBufferedContentAsync();

        var result = await buffered.ToBufferedContentAsync();

        Assert.Same(buffered, result);
    }

    [Fact]
    public void ToCompressed_WhenCompressionMethodIsUnknown_ThrowsArgumentOutOfRangeException()
    {
        using var content = new StringContent("payload");

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            content.ToCompressed((CompressionMethod)999));

        Assert.Equal("compressionMethod", ex.ParamName);
    }

    [Fact]
    public async Task FromJson_CreatesUtf8JsonContent()
    {
        using var content = HttpContent.FromJson("""{"name":"fclex"}""");

        var json = await content.ReadAsStringAsync();

        Assert.Equal("""{"name":"fclex"}""", json);
        Assert.Equal(MediaTypes.Json, content.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, content.Headers.ContentType?.CharSet);
    }

    [Fact]
    public async Task Json_SerializesObjectAsUtf8JsonContent()
    {
        using var content = HttpContent.Json(new JsonModel("fclex", 1));

        var json = await content.ReadAsStringAsync();

        Assert.Equal("""{"Name":"fclex","Value":1}""", json);
        Assert.Equal(MediaTypes.Json, content.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, content.Headers.ContentType?.CharSet);
    }

    private sealed record JsonModel(string Name, int Value);
}

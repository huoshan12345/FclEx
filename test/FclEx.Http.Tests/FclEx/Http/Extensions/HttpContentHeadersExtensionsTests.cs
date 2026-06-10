namespace FclEx.Http.Extensions;

public class HttpContentHeadersExtensionsTests
{
    [Fact]
    public void CopyTo_CopiesAllHeadersWhenNoHeaderIsExcluded()
    {
        using var source = new StringContent("source");
        using var destination = new StringContent("destination");
        source.Headers.TryAddWithoutValidation("X-Custom", ["one", "two"]);

        source.Headers.CopyTo(destination.Headers);

        Assert.Equal(["one", "two"], destination.Headers.GetValues("X-Custom"));
        Assert.Equal(source.Headers.ContentType, destination.Headers.ContentType);
    }

    [Fact]
    public void CopyTo_ExcludesHeadersCaseInsensitively()
    {
        using var source = new StringContent("source", Encoding.UTF8, MediaTypes.Text);
        using var destination = new ByteArrayContent([]);
        source.Headers.TryAddWithoutValidation("X-Custom", "value");

        source.Headers.CopyTo(destination.Headers, "content-type", "x-custom");

        Assert.False(destination.Headers.Contains("X-Custom"));
        Assert.Null(destination.Headers.ContentType);
    }
}

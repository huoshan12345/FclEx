namespace FclEx.Http.Extensions;

public class HttpContentExtensionsTests
{
    [Fact]
    public async Task ReadAsStreamAsync_WhenContentLengthExceedsInt32MaxValue_ThrowsClearError()
    {
        using var content = new StringContent("");
        content.Headers.ContentLength = (long)int.MaxValue + 1;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            content.ReadAsStreamAsync(null, null, CancellationToken.None));

        Assert.Contains("Content length is too large", ex.Message);
    }
}

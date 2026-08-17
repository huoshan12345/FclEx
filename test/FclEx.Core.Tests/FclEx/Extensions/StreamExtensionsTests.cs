namespace FclEx.Extensions;

public class StreamExtensionsTests
{
    [Fact]
    public async Task ReadAllTextAsync_LeavesTheCallerStreamOpenByDefault()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("text"));

        var text = await stream.ReadAllTextAsync();

        Assert.Equal("text", text);
        Assert.True(stream.CanRead);
    }
}

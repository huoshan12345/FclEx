namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
public class StreamReaderExtensionsTests
{
    [Fact]
    public async Task ReadToEndAsync_ShouldReturnFullContent()
    {
        var input = "Hello, world!\nThis is a test.";
        using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        var result = await reader.ReadToEndAsync(CancellationToken.None);

        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ReadLineAsync_ShouldReturnLinesSequentially()
    {
        var input = "Line1\nLine2\nLine3";
        using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        var line1 = await reader.ReadLineAsync(CancellationToken.None);
        var line2 = await reader.ReadLineAsync(CancellationToken.None);
        var line3 = await reader.ReadLineAsync(CancellationToken.None);
        var line4 = await reader.ReadLineAsync(CancellationToken.None); // should be null (EOF)

        Assert.Equal("Line1", line1);
        Assert.Equal("Line2", line2);
        Assert.Equal("Line3", line3);
        Assert.Null(line4);
    }

    [Fact]
    public async Task ReadToEndAsync_ShouldRespectCancellation()
    {
        var input = new string('x', 10000);
        using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

#if NET7_0_OR_GREATER
        await Assert.ThrowsAsync<TaskCanceledException>(async ()
            => await reader.ReadToEndAsync(cts.Token));
#else
        await reader.ReadToEndAsync(cts.Token);
#endif
    }

    [Fact]
    public async Task ReadLineAsync_ShouldRespectCancellation()
    {
        // Arrange
        var input = "Line1\nLine2\nLine3";
        using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

#if NET7_0_OR_GREATER
        await Assert.ThrowsAsync<TaskCanceledException>(async ()
            => await reader.ReadLineAsync(cts.Token));
#else
        await reader.ReadLineAsync(cts.Token);
#endif

    }
}

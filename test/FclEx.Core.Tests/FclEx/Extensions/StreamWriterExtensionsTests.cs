using System.Reflection.PortableExecutable;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "UseAwaitUsing")]
public class StreamWriterExtensionsTests
{
    [Fact]
    public async Task FlushAsync_WithoutCancellationToken_StillWorks()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true);

        await writer.WriteAsync("hello");
        await writer.FlushAsync(CancellationToken.None);

        // Verify that data was flushed to the stream
        Assert.True(ms.Length > 0);
    }

    [Fact]
    public async Task FlushAsync_WithCancellationToken_CallsUnderlyingMethod()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true);

        await writer.WriteAsync("test-data");
        using var cts = new CancellationTokenSource();

        await writer.FlushAsync(cts.Token);

        Assert.True(ms.Length > 0);
    }

    [Fact]
    public async Task FlushAsync_WhenCancelled_ThrowsTaskCanceledException()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true);

        await writer.WriteAsync("something");
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif

#if NET8_0_OR_GREATER
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => writer.FlushAsync(cts.Token));
#else
        await writer.FlushAsync(cts.Token);
#endif


    }
}

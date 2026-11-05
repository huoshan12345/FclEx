#if !NET6_0_OR_GREATER
namespace FclEx.Serilog.Extensions;

public class LogExtensionsTests
{
    [Fact(DisableParallelization = true)]
    public async Task CloseAndFlushAsync_CompatibilityMethod_CompletesSynchronousDisposal()
    {
        var original = Log.Logger;
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.Sink(sink.Object).CreateLogger();
        try
        {
            Log.Logger = logger;

            var shutdown = Log.CloseAndFlushAsync();

            Assert.True(shutdown.IsCompleted);
            await shutdown;
            Assert.NotSame(logger, Log.Logger);
            sink.As<IDisposable>().Verify(value => value.Dispose(), Times.Once);
        }
        finally
        {
            Log.Logger = original;
        }
    }
}
#endif

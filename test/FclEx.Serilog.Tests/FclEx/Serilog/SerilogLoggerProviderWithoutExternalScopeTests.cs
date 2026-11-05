#if NET6_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public class SerilogLoggerProviderWithoutExternalScopeTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DisposeProviderAsync_OwnedLogger_DisposesAsyncSink(bool useWrapper)
    {
        var sink = new Mock<ILogEventSink>();
        var asynchronous = sink.As<IAsyncDisposable>();
        asynchronous.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var logger = new LoggerConfiguration().WriteTo.Sink(sink.Object).CreateLogger();
        using var provider = new ServiceCollection().AddLogging()
            .AddSingleton<ILoggerProvider>(services => useWrapper
                ? new SerilogLoggerProviderWithoutExternalScope(logger, dispose: true)
                : new global::Serilog.Extensions.Logging.SerilogLoggerProvider(logger, dispose: true))
            .BuildServiceProvider();
        provider.GetRequiredService<ILoggerFactory>();

        await provider.DisposeAsync();

        asynchronous.Verify(m => m.DisposeAsync(), Times.Once);
    }
}
#endif

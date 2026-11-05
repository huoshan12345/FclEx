using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public class SerilogLoggerProviderWithoutExternalScopeTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Dispose_RespectsLoggerOwnership(bool ownsLogger)
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        using var logger = new LoggerConfiguration().WriteTo.Sink(sink.Object).CreateLogger();
        var provider = new SerilogLoggerProviderWithoutExternalScope(logger, dispose: ownsLogger);

        provider.Dispose();

        sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), ownsLogger ? Times.Once() : Times.Never());
    }

#if NET6_0_OR_GREATER
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DisposeAsync_RespectsLoggerOwnership(bool ownsLogger)
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        sink.As<IAsyncDisposable>().Setup(disposable => disposable.DisposeAsync()).Returns(ValueTask.CompletedTask);
        await using var logger = new LoggerConfiguration().WriteTo.Sink(sink.Object).CreateLogger();
        var provider = new SerilogLoggerProviderWithoutExternalScope(logger, dispose: ownsLogger);

        await provider.DisposeAsync();

        sink.As<IAsyncDisposable>().Verify(disposable => disposable.DisposeAsync(), ownsLogger ? Times.Once() : Times.Never());
        sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Never);
    }

    [Fact]
    public async Task DisposeAsync_SynchronousOnlySink_DisposesOwnedLogger()
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.Sink(sink.Object).CreateLogger();
        var provider = new SerilogLoggerProviderWithoutExternalScope(logger, dispose: true);

        await provider.DisposeAsync();

        sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Once);
    }

    [Fact]
    public async Task DisposeProviderAsync_WaitsForOwnedLogger()
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sink = new Mock<ILogEventSink>();
        sink.As<IAsyncDisposable>().Setup(disposable => disposable.DisposeAsync()).Returns(new ValueTask(completion.Task));
        var logger = new LoggerConfiguration().WriteTo.Sink(sink.Object).CreateLogger();
        var services = new ServiceCollection().AddLogging()
            .AddSingleton<ILoggerProvider>(_ => new SerilogLoggerProviderWithoutExternalScope(logger, dispose: true));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ILoggerFactory>();

        var disposal = provider.DisposeAsync().AsTask();

        Assert.False(disposal.IsCompleted);
        completion.SetResult(true);
        await disposal;
        sink.As<IAsyncDisposable>().Verify(disposable => disposable.DisposeAsync(), Times.Once);
    }

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
#endif
}

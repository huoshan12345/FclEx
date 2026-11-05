using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

public partial class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSerilog_NullConfigure_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddSerilog((Action<LoggerConfiguration>)null!));
    }

    [Fact]
    public void AddSerilog_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddSerilog(new SerilogConfiguration()));
    }

    [Fact]
    public void AddSerilog_CreatesLoggerOnFirstResolution()
    {
        var creations = 0;
        var services = new ServiceCollection().AddSerilog(logger => creations++);
        using var provider = services.BuildServiceProvider();
        Assert.Equal(0, creations);

        var first = provider.GetRequiredService<global::Serilog.ILogger>();
        var second = provider.GetRequiredService<global::Serilog.ILogger>();

        Assert.Equal(1, creations);
        Assert.Same(first, second);
    }

    [Fact]
    public void DisposeProvider_UnusedLogger_CreatesNoLogger()
    {
        var creations = 0;
        var services = new ServiceCollection().AddSerilog(logger => creations++);

        services.BuildServiceProvider().Dispose();

        Assert.Equal(0, creations);
    }

    [Fact]
    public void AddSerilog_ReplacingRegistration_CreatesOnlyLatestLogger()
    {
        var firstCreations = 0;
        var secondCreations = 0;
        var services = new ServiceCollection()
            .AddSerilog(logger => firstCreations++)
            .AddSerilog(logger => secondCreations++);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<global::Serilog.ILogger>();

        Assert.Equal(0, firstCreations);
        Assert.Equal(1, secondCreations);
    }

    [Fact]
    public void DisposeProvider_ResolvedMicrosoftLogger_DisposesSinkOnce()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var provider = new ServiceCollection().AddSerilog(logger => logger.WriteTo.Sink(sink.Object))
            .BuildServiceProvider();
        provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();
        provider.GetRequiredService<global::Serilog.ILogger>();

        provider.Dispose();

        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void DisposeLoggerFactory_ContainerRetainsLoggerOwnership()
    {
        var sink = new TrackingLifetimeSink();
        var provider = new ServiceCollection().AddSerilog(logger => logger.WriteTo.Sink(sink))
            .BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var logger = provider.GetRequiredService<global::Serilog.ILogger>();

        factory.Dispose();
        logger.Information("still owned by the container");

        Assert.Equal(0, sink.DisposeCount);
        Assert.Single(sink.Events);
        provider.Dispose();
        Assert.Equal(1, sink.DisposeCount);
    }

    [Fact]
    public void DisposeScope_RootContainerRetainsLoggerOwnership()
    {
        var sink = new TrackingLifetimeSink();
        using var provider = new ServiceCollection().AddSerilog(logger => logger.WriteTo.Sink(sink))
            .BuildServiceProvider();
        var rootLogger = provider.GetRequiredService<global::Serilog.ILogger>();

        using (var scope = provider.CreateScope())
            Assert.Same(rootLogger, scope.ServiceProvider.GetRequiredService<global::Serilog.ILogger>());

        Assert.Equal(0, sink.DisposeCount);
        rootLogger.Information("root logger remains active");
        Assert.Single(sink.Events);
        provider.Dispose();
        Assert.Equal(1, sink.DisposeCount);
    }

    [Fact]
    public void AddSerilog_MultipleProviders_OwnIndependentLoggers()
    {
        var sinks = new List<TrackingLifetimeSink>();
        var services = new ServiceCollection().AddSerilog(logger =>
        {
            var sink = new TrackingLifetimeSink();
            sinks.Add(sink);
            logger.WriteTo.Sink(sink);
        });
        using var firstProvider = services.BuildServiceProvider();
        using var secondProvider = services.BuildServiceProvider();
        var first = firstProvider.GetRequiredService<global::Serilog.ILogger>();
        var second = secondProvider.GetRequiredService<global::Serilog.ILogger>();
        Assert.NotSame(first, second);
        Assert.Equal(2, sinks.Count);

        firstProvider.Dispose();
        second.Information("second container remains active");

        Assert.Equal(1, sinks[0].DisposeCount);
        Assert.Equal(0, sinks[1].DisposeCount);
        Assert.Single(sinks[1].Events);
        secondProvider.Dispose();
        Assert.Equal(1, sinks[1].DisposeCount);
    }

    [Fact]
    public async Task AddSerilog_ConcurrentResolution_CreatesOneLogger()
    {
        var creations = 0;
        using var provider = new ServiceCollection()
            .AddSerilog(logger => Interlocked.Increment(ref creations))
            .BuildServiceProvider();

        var loggers = await Task.WhenAll(Enumerable.Range(0, 16)
            .Select(index => Task.Run(() => provider.GetRequiredService<global::Serilog.ILogger>())));

        Assert.Equal(1, creations);
        Assert.All(loggers, logger => Assert.Same(loggers[0], logger));
    }

    [Fact]
    public void AddSerilog_PreservesGlobalLogger()
    {
        var globalLogger = Log.Logger;
        var services = new ServiceCollection().AddSerilog();
        Assert.Same(globalLogger, Log.Logger);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<global::Serilog.ILogger>();

        Assert.Same(globalLogger, Log.Logger);
    }

    [Fact]
    public void DisposeProvider_FlushesBatchedEvents()
    {
        var events = new List<LogEvent>();
        var sink = new Mock<IBatchedLogEventSink>();
        sink.Setup(m => m.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()))
            .Callback<IReadOnlyCollection<LogEvent>>(batch => events.AddRange(batch))
            .Returns(Task.CompletedTask);
        var provider = new ServiceCollection().AddSerilog(logger => logger.WriteTo.Sink(sink.Object,
            new global::Serilog.Configuration.BatchingOptions
            {
                EagerlyEmitFirstEvent = false,
                BufferingTimeLimit = TimeSpan.FromHours(1)
            })).BuildServiceProvider();
        provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>().LogInformation("buffered event");

        provider.Dispose();

        Assert.Equal("buffered event", Assert.Single(events).RenderMessage());
    }

    [Fact]
    public void ResolveLogger_AfterConfigurationFailure_CreatesFreshPipeline()
    {
        var exception = new InvalidOperationException("first creation failed");
        var sinks = new List<TrackingLifetimeSink>();
        using var provider = new ServiceCollection().AddSerilog(logger =>
        {
            var sink = new TrackingLifetimeSink();
            sinks.Add(sink);
            logger.WriteTo.Sink(sink);
            if (sinks.Count == 1)
                throw exception;
        }).BuildServiceProvider();

        var actual = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<global::Serilog.ILogger>());
        Assert.Same(exception, actual);
        Assert.Equal(1, Assert.Single(sinks).DisposeCount);
        var logger = provider.GetRequiredService<global::Serilog.ILogger>();
        logger.Information("retry succeeded");

        Assert.Equal(2, sinks.Count);
        Assert.Single(sinks[1].Events);
        Assert.Equal(0, sinks[1].DisposeCount);
        provider.Dispose();
        Assert.Equal(1, sinks[1].DisposeCount);
    }

#if NET6_0_OR_GREATER
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DisposeProviderAsync_UsesAsyncSinkDisposal(bool supportsSynchronousDisposal)
    {
        var sink = new Mock<ILogEventSink>();
        var asynchronous = sink.As<IAsyncDisposable>();
        asynchronous.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var synchronous = supportsSynchronousDisposal ? sink.As<IDisposable>() : null;
        var provider = new ServiceCollection().AddSerilog(logger => logger.WriteTo.Sink(sink.Object))
            .BuildServiceProvider();
        provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();

        await provider.DisposeAsync();

        asynchronous.Verify(m => m.DisposeAsync(), Times.Once);
        synchronous?.Verify(m => m.Dispose(), Times.Never);
    }

    [Fact]
    public async Task DisposeProviderAsync_WaitsForSinkDisposal()
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sink = new Mock<ILogEventSink>();
        var asynchronous = sink.As<IAsyncDisposable>();
        asynchronous.Setup(m => m.DisposeAsync()).Returns(new ValueTask(completion.Task));
        var provider = new ServiceCollection().AddSerilog(logger => logger.WriteTo.Sink(sink.Object))
            .BuildServiceProvider();
        provider.GetRequiredService<global::Serilog.ILogger>();

        var disposal = provider.DisposeAsync().AsTask();

        Assert.False(disposal.IsCompleted);
        completion.SetResult(true);
        await disposal;
        asynchronous.Verify(m => m.DisposeAsync(), Times.Once);
    }
#endif

    private sealed class TrackingLifetimeSink : ILogEventSink, IDisposable
    {
        public List<LogEvent> Events { get; } = [];
        public int DisposeCount { get; private set; }

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
        public void Dispose() => DisposeCount++;
    }
}

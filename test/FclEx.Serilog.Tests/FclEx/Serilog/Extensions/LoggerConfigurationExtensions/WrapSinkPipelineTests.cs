using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

public class WrapSinkPipelineTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WrapSinkPipeline_NormalizesOnceBeforeAllSinks(bool formatException)
    {
        var mutations = 0;
        var observed = new List<(LogEventLevel Level, Exception? Exception)>();
        var first = new Mock<ILogEventSink>();
        first.Setup(value => value.Emit(It.IsAny<LogEvent>()))
            .Callback<LogEvent>(value => observed.Add((value.Level, value.Exception)));
        var second = new Mock<ILogEventSink>();
        second.Setup(value => value.Emit(It.IsAny<LogEvent>()))
            .Callback<LogEvent>(value => observed.Add((value.Level, value.Exception)));
        using var logger = new LoggerConfiguration().WriteTo.Sink(first.Object).WriteTo.Sink(second.Object)
            .WrapSinkPipeline(inner => new LogEventMutateSink(inner, value =>
            {
                mutations++;
                if (formatException)
                    value.FormatException();
            })).CreateLogger();
        var nested = new LogException("failure", LogLevel.Warning, new InvalidOperationException("failure"));

        logger.Error(new LogException("failure", LogLevel.Error, nested), "failure");

        Assert.Equal(1, mutations);
        Assert.Equal(2, observed.Count);
        Assert.All(observed, value => Assert.Equal(LogEventLevel.Error, value.Level));
        Assert.Same(observed[0].Exception, observed[1].Exception);
        if (formatException)
            Assert.Same(nested, Assert.IsType<FormattedException>(observed[0].Exception).Exception);
        else
            Assert.Same(nested, observed[0].Exception);
    }

    [Fact]
    public void WrapSinkPipeline_PreservesGlobalFiltersAndPerSinkRestrictions()
    {
        Exception? filteredException = null;
        var warning = new CollectingSink();
        var error = new CollectingSink();
        var exception = new LogException("failure", LogLevel.Warning);
        using var logger = new LoggerConfiguration().Filter.ByIncludingOnly(value =>
            {
                filteredException = value.Exception;
                return true;
            })
            .WriteTo.Sink(warning, restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.Sink(error, restrictedToMinimumLevel: LogEventLevel.Error)
            .WrapSinkPipeline(inner => new LogEventMutateSink(inner, value => value.FormatException())).CreateLogger();

        logger.Error(exception, "message");

        Assert.Same(exception, filteredException);
        Assert.Equal(LogEventLevel.Warning, Assert.Single(warning.Events).Level);
        Assert.IsType<FormattedException>(warning.Events[0].Exception);
        Assert.Empty(error.Events);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WrapSinkPipeline_DoesNotCaptureLaterOrAuditSinks(bool audit)
    {
        var captured = new CollectingSink();
        var outside = new CollectingSink();
        var configuration = new LoggerConfiguration().WriteTo.Sink(captured);
        if (audit)
            configuration.AuditTo.Sink(outside);
        configuration.WrapSinkPipeline(inner => new SuppressingSink(inner));
        if (!audit)
            configuration.WriteTo.Sink(outside);
        using var logger = configuration.CreateLogger();

        logger.Information("message");

        Assert.Empty(captured.Events);
        Assert.Single(outside.Events);
    }

    [Fact]
    public void WrapSinkPipeline_EmptyPipeline_StillInvokesWrapperOnce()
    {
        var factories = 0;
        var mutations = 0;
        using var logger = new LoggerConfiguration().WrapSinkPipeline(inner =>
        {
            factories++;
            return new LogEventMutateSink(inner, _ => mutations++);
        }).CreateLogger();

        logger.Information("message");

        Assert.Equal(1, factories);
        Assert.Equal(1, mutations);
    }

    [Fact]
    public void WrapSinkPipeline_NullFactory_DoesNotDetachConfiguredSinks()
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        var configuration = new LoggerConfiguration().WriteTo.Sink(sink.Object);

        Assert.Throws<ArgumentNullException>(() => configuration.WrapSinkPipeline(null!));
        configuration.CreateLogger().Dispose();

        sink.As<IDisposable>().Verify(value => value.Dispose(), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WrapSinkPipeline_FactoryFails_ReleasesAndDetachesCapturedSinks(bool returnsNull)
    {
        var failure = new InvalidOperationException("wrapper failed");
        var asynchronous = new Mock<ILogEventSink>();
        asynchronous.As<IAsyncDisposable>().Setup(value => value.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var synchronous = new Mock<ILogEventSink>();
        synchronous.As<IDisposable>();
        var configuration = new LoggerConfiguration().WriteTo.Sink(asynchronous.Object).WriteTo.Sink(synchronous.Object);

        var actual = Assert.Throws<InvalidOperationException>(() => configuration.WrapSinkPipeline(_ => returnsNull ? null! : throw failure));
        // A failed wrapping operation must not leave disposed registrations behind for a second cleanup.
        configuration.CreateLogger().Dispose();

        if (!returnsNull)
            Assert.Same(failure, actual);
        asynchronous.As<IAsyncDisposable>().Verify(value => value.DisposeAsync(), Times.Once);
        synchronous.As<IDisposable>().Verify(value => value.Dispose(), Times.Once);
    }

    private sealed class SuppressingSink(ILogEventSink inner) : LogEventMutateSink(inner, null)
    {
        public override void Emit(LogEvent logEvent) { }
    }
}

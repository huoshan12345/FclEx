using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddSerilog_Test(bool formatException)
    {
        using var listener = new LogEventListener();

        var provider = new ServiceCollection()
            .AddSerilog((m, n) =>
            {
                n.WriteTo(listener)
                    .Enrich(new LogEnricher(nameof(AddSerilog_Test)))
                    .FormatException(formatException);
            })
            .BuildServiceProvider();

        var logger = provider.GetService<Microsoft.Extensions.Logging.ILogger>();
        Assert.NotNull(logger);

        var loggerFactory = provider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);

        var typedLogger = provider.GetService<ILogger<ServiceCollectionExtensionsTests>>();
        Assert.NotNull(typedLogger);

        var serilogLogger = provider.GetService<global::Serilog.ILogger>();
        Assert.NotNull(serilogLogger);

        serilogLogger.Information(new LogException("test", LogEventLevel.Warning), "");

        var flag = await listener.WaitAsync(1, TimeSpan.FromSeconds(1));
        Assert.True(flag);

        var logEvent = listener.Events.First();
        Assert.Equal(LogEventLevel.Warning, logEvent.Level);

        if (formatException)
        {
            Assert.IsType<FormattedException>(logEvent.Exception);
        }
        else
        {
            Assert.IsType<LogException>(logEvent.Exception);
        }
    }
}

using FclEx.Helpers;

namespace FclEx.Logging.Extensions;

public class LoggerFactoryExtensionsTests
{
    public static readonly TheoryData<LogLevel> LogLevelCases = EnumHelper.GetValues<LogLevel>().ToTheoryData();

    [Theory]
    [MemberData(nameof(LogLevelCases))]
    public void SetMinimumLevel_Test(LogLevel logLevel)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var fac = services.GetRequiredService<ILoggerFactory>();
        fac.SetMinimumLevel(logLevel);

        var options = (LoggerFilterOptions?)fac.GetType().InvokeMember(
            name: "_filterOptions",
            invokeAttr: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
            binder: null,
            target: fac,
            args: null);

        Assert.Equal(logLevel, options?.MinLevel);
    }
}
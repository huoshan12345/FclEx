namespace FclEx.Xunit;

public class XunitLoggerProvider : ILoggerProvider
{
    private readonly Func<ITestOutputHelper?> _outputResolver;
    private readonly bool _consoleFallback;

    public XunitLoggerProvider(Func<ITestOutputHelper?> outputResolver, bool consoleFallback)
    {
        _outputResolver = outputResolver;
        _consoleFallback = consoleFallback;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(categoryName, _outputResolver, _consoleFallback);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
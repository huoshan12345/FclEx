namespace Microsoft.Extensions.Logging;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    private readonly bool _needToCheckDisposed;

    public TestLoggerProvider(ITestOutputHelper output, bool needToCheckDisposed)
    {
        _output = output;
        _needToCheckDisposed = needToCheckDisposed;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(_output, categoryName, _needToCheckDisposed);
    }
}
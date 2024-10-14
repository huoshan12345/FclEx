namespace Microsoft.Extensions.Logging;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    private readonly bool _checkDisposed;

    public TestLoggerProvider(ITestOutputHelper output, bool checkDisposed)
    {
        _output = output;
        _checkDisposed = checkDisposed;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(_output, categoryName, _checkDisposed);
    }
}
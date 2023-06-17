using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Extensions.Logging
{
    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _output;
        private readonly string _name;
        private readonly bool _needToCheckDisposed;
        private readonly object _lock = new();
        private bool _isDisposed;

        private static readonly FieldInfo _field = typeof(TestOutputHelper).GetField("buffer", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public TestLogger(ITestOutputHelper output, string name, bool needToCheckDisposed)
        {
            _output = output;
            _name = name;
            _needToCheckDisposed = needToCheckDisposed;
        }

        private static bool CheckDisposed(ITestOutputHelper output)
        {
            if (output is TestOutputHelper helper)
            {
                return _field.GetValue(helper) == null;
            }
            else
            {
                return false;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
        {
            if (!IsEnabled(logLevel) || formatter == null || _isDisposed) return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null) return;

            if (_needToCheckDisposed)
            {
                if (_isDisposed) return;
                lock (_lock)
                {
                    if (_isDisposed) return;

                    if (CheckDisposed(_output))
                    {
                        _isDisposed = true;
                        return;
                    }
                }
            }

            var msg = exception is null ? message : message + Environment.NewLine + exception;
            _output.WriteLine($"[{_name}][{logLevel}]" + msg);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return EmptyDisposable.Instance;
        }
    }
}

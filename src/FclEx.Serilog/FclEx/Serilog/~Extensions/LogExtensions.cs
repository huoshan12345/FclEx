namespace FclEx.Serilog;

public static class LogExtensions
{
    extension(Log)
    {
#if !NET6_0_OR_GREATER
        public static ValueTask CloseAndFlushAsync()
        {
            Log.CloseAndFlush();
            return ValueTask.CompletedTask;
        }
#endif
    }
}

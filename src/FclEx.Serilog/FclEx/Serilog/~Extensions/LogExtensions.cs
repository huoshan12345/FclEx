namespace FclEx.Serilog;

/// <summary>Backfills global logger shutdown APIs on targets where Serilog lacks them.</summary>
public static class LogExtensions
{
    extension(Log)
    {
#if !NET6_0_OR_GREATER
        /// <summary>Closes and flushes the global logger synchronously on older targets.</summary>
        /// <returns>An already-completed ValueTask after synchronous CloseAndFlush finishes.</returns>
        /// <remarks>This compatibility method does not perform asynchronous sink cleanup. Shutdown exceptions propagate synchronously.</remarks>
        public static ValueTask CloseAndFlushAsync()
        {
            Log.CloseAndFlush();
            return ValueTask.CompletedTask;
        }
#endif
    }
}

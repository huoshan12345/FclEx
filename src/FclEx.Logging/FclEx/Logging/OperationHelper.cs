namespace FclEx.Logging;

public static class OperationHelper
{
    private static async Task<T?> RunAsync<T>(string title, Func<Task<T>> task, ILogger logger, bool throwOnError)
    {
        var watch = ValueStopwatch.StartNew();

        using var a = logger.Properties(LogPropertyNames.Operation, title);
        try
        {
            var result = await task();
            logger.LogOperation(title, watch.GetElapsedTime());
            return result;
        }
        catch (Exception ex)
        {
            logger.LogOperationError(ex, title, watch.GetElapsedTime());

            if (throwOnError)
                throw;

            return default;
        }
    }

    public static Task RunAsyncSafely(string title, Func<Task> task, ILogger logger)
    {
        return RunAsync(title, () => task().Then(() => Unit.Default), logger, false);
    }

    public static Task RunAsync(string title, Func<Task> task, ILogger logger)
    {
        return RunAsync(title, () => task().Then(() => Unit.Default), logger, true);
    }

    public static Task<T?> RunAsyncSafely<T>(string title, Func<Task<T>> task, ILogger logger)
    {
        return RunAsync(title, task, logger, false);
    }

    public static Task<T> RunAsync<T>(string title, Func<Task<T>> task, ILogger logger)
    {
        return RunAsync(title, task, logger, true)!;
    }
}

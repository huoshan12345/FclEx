using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace FclEx.Extensions;

public static class LoggerFactoryExtensions
{
    private static readonly FieldInfo FieldOfFilterOptions = typeof(LoggerFactory).GetField("_filterOptions", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static void SetMinimumLevel(this ILoggerFactory factory, LogLevel minLevel)
    {
        Check.NotNull(factory);

        if (factory is LoggerFactory fac)
        {
            var options = (LoggerFilterOptions)FieldOfFilterOptions.GetValue(fac)!;
            options.MinLevel = minLevel;
        }
        else
        {
            throw new NotSupportedException("Not supported logger factory type: " + factory.GetType().LongName());
        }
    }
}
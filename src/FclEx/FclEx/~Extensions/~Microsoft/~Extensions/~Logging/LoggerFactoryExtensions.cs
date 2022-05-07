using System;
using System.Reflection;
using Dawn;
using Microsoft.Extensions.Logging;

namespace FclEx
{
    public static class LoggerFactoryExtensions
    {
        private static readonly FieldInfo FieldOfFilterOptions = typeof(LoggerFactory).GetField("_filterOptions", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static void SetMinimumLevel(this ILoggerFactory factory, LogLevel minLevel)
        {
            Guard.Argument(factory, nameof(factory)).NotNull();

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
}
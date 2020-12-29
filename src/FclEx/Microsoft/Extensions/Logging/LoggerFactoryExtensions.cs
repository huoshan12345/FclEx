using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dawn;
using FclEx;
using FclEx.Utils;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Extensions.Logging
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
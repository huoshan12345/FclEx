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
    public static class LoggerProviderExtensions
    {
        public static ILogger CreateLogger(this ILoggerProvider provider, Type t)
        {
            var name = t.LongName();
            return provider.CreateLogger(name);
        }

        public static ILogger CreateLogger<T>(this ILoggerProvider provider)
        {
            return provider.CreateLogger(typeof(T));
        }
    }
}
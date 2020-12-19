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
    public static class Extensions
    {
        public static bool IsNullOrNullLogger([NotNullWhen(false)] this ILogger? logger)
        {
            if (logger == null) return true;

            var type = logger.GetType();

            return type == typeof(NullLogger)
                   || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NullLogger<>);
        }

        public static ILogger With(this ILogger logger, IEnumerable<KeyValuePair<string, object>> properties)
        {
            return new PropertiesLogger(logger, properties);
        }

        public static ILogger With(this ILogger logger, params KeyValuePair<string, object>[] properties)
        {
            return logger.With(properties.AsEnumerable());
        }

        public static ILogger With(this ILogger logger, IEnumerable<(string, object)> properties)
        {
            return logger.With(properties.Select(m => KvPair.Create(m.Item1, m.Item2)));
        }

        public static ILogger With(this ILogger logger, params (string, object)[] properties)
        {
            return logger.With(properties.AsEnumerable());
        }

        public static ILogger With(this ILogger logger, string key, object value)
        {
            return logger.With(KvPair.Create(key, value));
        }

        public static ILogger With(this ILogger logger, (string key, object value) prop)
        {
            return logger.With(prop.key, prop.value);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IDisposable PushProperty(this ILogger logger, IEnumerable<KeyValuePair<string, object>> properties)
        {
            return properties.IsNullOrEmpty() ? EmptyDisposable.Instance : logger.BeginScope(properties);
        }

        public static IDisposable PushProperty<T>(this ILogger logger, IEnumerable<KeyValuePair<string, T>> properties)
        {
            return logger.PushProperty(properties.Touch().Select(m => KvPair.Create(m.Key, (object)m.Value!)));
        }

        public static IDisposable PushProperty(this ILogger logger, params KeyValuePair<string, object>[] properties)
        {
            return logger.PushProperty(properties.Touch().AsEnumerable());
        }

        public static IDisposable PushProperty(this ILogger logger, IEnumerable<(string, object)> properties)
        {
            return logger.PushProperty(properties.Touch().AsKeyValue());
        }

        public static IDisposable PushProperty<T>(this ILogger logger, IEnumerable<(string, T)> properties)
        {
            return logger.PushProperty(properties.Touch().Select(m => (m.Item1, (object)m.Item2!)));
        }

        public static IDisposable PushProperty(this ILogger logger, params (string, object)[] properties)
        {
            return logger.PushProperty(properties.Touch().AsEnumerable());
        }

        public static IDisposable PushProperty(this ILogger logger, string key, object value)
        {
            return logger.PushProperty(KvPair.Create(key, value));
        }

        public static IDisposable PushProperty(this ILogger logger, (string key, object value) prop)
        {
            return logger.PushProperty(prop.key, prop.value);
        }
    }
}
using System;
using FclEx;
using FclEx.Extensions;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddJsonFileIf(this IConfigurationBuilder builder, bool condition,
            string path, bool optional, bool reloadOnChange)
        {
            return condition ? builder.AddJsonFile(path, optional, reloadOnChange) : builder;
        }

        public static bool IfPathExists(this IConfigurationBuilder builder, string path)
        {
            return builder.GetFileProvider().GetFileInfo(path).Exists;
        }

        public static IConfigurationBuilder AddJsonFileIf(this IConfigurationBuilder builder, bool condition,
            string path, string fallBackPath, bool optional, bool reloadOnChange)
        {
            if (!condition) return builder;
            var actualPath = builder.IfPathExists(path) ? path : fallBackPath;
            return builder.AddJsonFile(actualPath, optional, reloadOnChange);
        }

        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder,
            string path, string fallBackPath, bool optional, bool reloadOnChange)
        {
            var actualPath = builder.IfPathExists(path) ? path : fallBackPath;
            return builder.AddJsonFile(actualPath, optional, reloadOnChange);
        }

        public static IConfiguration Merge(this IConfiguration x, IConfiguration y)
        {
            return new ConfigurationBuilder()
                .AddConfiguration(x)
                .AddConfiguration(y)
                .Build();
        }

        public static T GetRequiredValue<T>(this IConfiguration config, string key)
        {
            var section = config.GetSection(key);
            if (section == null)
                throw new InvalidOperationException("Can not find section by key: " + key);
            var obj = section.Get<T>();
            if (obj == null)
                throw new InvalidOperationException($"Can not get a non-null instance of type {typeof(T).ShortName()} by key: " + key);
            return obj;
        }
    }
}

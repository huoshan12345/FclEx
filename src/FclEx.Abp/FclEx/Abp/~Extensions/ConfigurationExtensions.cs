using System;
using Microsoft.Extensions.Configuration;

namespace FclEx.Abp;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonFileIf(
        this IConfigurationBuilder builder,
        bool condition,
        string path,
        bool optional,
        bool reloadOnChange)
    {
        return condition ? builder.AddJsonFile(path, optional, reloadOnChange) : builder;
    }

    public static bool IfPathExists(this IConfigurationBuilder builder, string path)
    {
        return builder.GetFileProvider().GetFileInfo(path).Exists;
    }

    public static IConfiguration Merge(this IConfiguration x, IConfiguration y)
    {
        return new ConfigurationBuilder()
            .AddConfiguration(x)
            .AddConfiguration(y)
            .Build();
    }

    public static T GetRequiredValue<T>(this IConfiguration config, string? key)
    {
        var section = key.IsNullOrEmpty() ? config : config.GetSection(key);
        if (section == null)
            throw new InvalidOperationException("Can not find section by key: " + key);

        var obj = section.Get<T>();
        return obj ?? throw new InvalidOperationException($"Can not get a non-null instance of type {typeof(T).ShortName()} by key: " + key);
    }
}
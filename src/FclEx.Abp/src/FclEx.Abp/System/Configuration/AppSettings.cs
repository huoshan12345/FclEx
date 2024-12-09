using System.Collections.Generic;


namespace System.Configuration;

public class AppSettings
{
    [return: NotNullIfNotNull("defaultTValue")]
    public static T? GetValue<T>(Func<string, T> parseFunc, string key, T? defaultTValue = default)
    {
        try
        {
            var node = ConfigurationManager.AppSettings[key];
            return !string.IsNullOrWhiteSpace(node) ? parseFunc(node) : defaultTValue;
        }
        catch
        {
            return defaultTValue;
        }
    }
        
    [return: NotNullIfNotNull("defaultTValue")]
    public static T? GetValue<T>(string key, Func<string, T> parseFunc, T? defaultTValue = default)
    {
        return GetValue(parseFunc, key, defaultTValue);
    }

    [return: NotNullIfNotNull("defaultTValue")]
    public static string GetValue(string key, string defaultTValue = "")
    {
        return GetValue(item => item, key, defaultTValue);
    }

    public static T GetRequiredValue<T>(Func<string, T> parseFunc, string key)
    {
        Check.NotNull(parseFunc);
        Check.NotNull(key);
        var node = ConfigurationManager.AppSettings[key]
                   ?? throw new KeyNotFoundException(key);
        return parseFunc(node);
    }

    public static string GetRequiredValue(string key)
    {
        return GetRequiredValue(item => item, key);
    }
}
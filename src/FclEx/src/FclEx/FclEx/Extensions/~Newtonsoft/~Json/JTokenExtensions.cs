namespace FclEx.Extensions;

public static class JTokenExtensions
{
    public static T RequiredValue<T>(this JToken token, string key)
    {
        var t = token[key] ?? throw new KeyNotFoundException($"Cannot find key '{key}'");
        return t.Value<T>() ?? throw new JsonSerializationException($"The value gotten by key '{key}' is null.");
    }

    public static T Value<T>(this JToken token, string key, T defaultValue)
    {
        var t = token[key];
        return t == null
            ? defaultValue 
            : t.Value<T>() ?? defaultValue;
    }
}
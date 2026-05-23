namespace FclEx.Domain;

public interface IKeyValueService
{
    IStringSerializer StringSerializer { get; }
    Task<string?> GetAsync(string objectId, string key, string? defaultValue = default);
    Task SaveAsync(string objectId, string key, string? value);
    Task RemoveAsync(string objectId, string key);
}

public static class KeyValueServiceExtensions
{
    public static async Task<T?> GetAsync<T>(this IKeyValueService service, string objectId, string key, T? defaultValue = default)
    {
        var v = await service.GetAsync(objectId, key);
        return v == null
            ? defaultValue
            : service.StringSerializer.Deserialize<T>(v);
    }

    public static async Task<T> SaveAsync<T>(this IKeyValueService service, string objectId, string key, T value)
    {
        var obj = value == null ? null : service.StringSerializer.Serialize(value);
        await service.SaveAsync(objectId, key, obj);
        return value;
    }

    public static async Task<T> GetRequiredAsync<T>(this IKeyValueService service, string objectId, string key, T defaultValue)
    {
        return await service.GetAsync(objectId, key, defaultValue)
               ?? throw new InvalidOperationException($"Cannot find value by objectId '{objectId}' and key '{key}'");
    }
}
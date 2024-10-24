namespace FclEx.Abp.Domain.Services;

public abstract class AbstractKeyValueService : IKeyValueService
{
    protected readonly IStringSerializer _stringSerializer;

    protected AbstractKeyValueService(IStringSerializer stringSerializer)
    {
        _stringSerializer = stringSerializer;
    }

    public abstract Task<string?> GetValue(string objectId, string key, string? defaultValue = default);
    public abstract Task<string?> AddOrUpdate(string objectId, string key, string? value);

    public virtual async Task<T?> GetValue<T>(string objectId, string key, T? defaultValue = default)
    {
        var v = await GetValue(objectId, key).IgnoreSyncContext();
        if (v == null) return defaultValue;
        return _stringSerializer.Deserialize<T>(v);
    }

    public virtual async Task<T> AddOrUpdate<T>(string objectId, string key, T value)
    {
        var obj = value == null ? null : _stringSerializer.Serialize(value);
        await AddOrUpdate(objectId, key, obj).IgnoreSyncContext();
        return value;
    }

    public abstract Task Remove(string objectId, string key);
    public abstract Task RemoveAll(string objectId);
}
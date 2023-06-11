using System.Threading.Tasks;

namespace FclEx.Abp.Domain.Services;

public interface IKeyValueService
{
    Task<string?> GetValue(string objectId, string key, string? defaultValue = default);
    Task<T?> GetValue<T>(string objectId, string key, T? defaultValue = default);
    Task<string?> AddOrUpdate(string objectId, string key, string value);
    Task<T> AddOrUpdate<T>(string objectId, string key, T value);
    Task Remove(string objectId, string key);
    Task RemoveAll(string objectId);
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Abp.Serializers;
using FclEx.Extensions;

namespace FclEx.Abp.Domain.Services
{
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
            var v = await GetValue(objectId, key).DonotCapture();
            if (v == null) return defaultValue;
            return _stringSerializer.Deserialize<T>(v);
        }

        public virtual async Task<T> AddOrUpdate<T>(string objectId, string key, T value)
        {
            var obj = value == null ? null : _stringSerializer.Serialize(value);
            await AddOrUpdate(objectId, key, obj).DonotCapture();
            return value;
        }

        public abstract Task Remove(string objectId, string key);
        public abstract Task RemoveAll(string objectId);
    }
}

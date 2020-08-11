using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FclEx.Helpers
{
    public static class JsonHelper
    {
        public static IContractResolver CamelResolver { get; } = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerSettings> _serializerSettings
            = new ConcurrentDictionary<JsonOptions, JsonSerializerSettings>();

        private static readonly ConcurrentDictionary<JsonOptions, JsonSerializer> _serializers
            = new ConcurrentDictionary<JsonOptions, JsonSerializer>();

        public static JsonSerializerSettings GetSettings(JsonOptions options)
        {
            return _serializerSettings.GetOrAdd(options, k =>
            {
                var settings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = k.DateTimeZoneHandling,
                    Formatting = k.Formatting,
                    NullValueHandling = k.IgnoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
                };
                if (k.DateTimeFormat.IsValid())
                    settings.DateFormatString = k.DateTimeFormat!;
                if (k.UseCamelCase)
                    settings.ContractResolver = CamelResolver;
                return settings;
            });
        }

        public static JsonSerializer CamelSerializer { get; } = GetSerializer(new JsonOptions(useCamelCase: true));

        public static JsonSerializer GetSerializer(JsonOptions options)
        {
            return _serializers.GetOrAdd(options, k => JsonSerializer.Create(GetSettings(k)));
        }
    }
}

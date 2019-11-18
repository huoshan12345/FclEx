using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Xml;
using System.Xml.Linq;
using FclEx.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace FclEx
{
    public static class JsonExtensions
    {
        internal static IContractResolver CamelResolver { get; } = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerSettings> _serializerSettings
            = new ConcurrentDictionary<JsonOptions, JsonSerializerSettings>();

        internal static JsonSerializerSettings GetSettings(JsonOptions options)
        {
            return _serializerSettings.GetOrAdd(options, k =>
            {
                var settings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = k.DateTimeZoneHandling,
                    Formatting = Formatting.None,
                    NullValueHandling = k.IgnoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
                };
                if (k.DateTimeFormat.IsValid())
                    settings.DateFormatString = k.DateTimeFormat;
                if (k.UseCamelCase)
                    settings.ContractResolver = CamelResolver;
                return settings;
            });
        }

        public static string ToJson(this object obj, JsonOptions options)
        {
            var settings = GetSettings(options);
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string ToJson(this object obj,
            Formatting formatting = Formatting.None,
            bool ignoreNull = false,
            DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Local,
            bool useCamelCase = false,
            string dateTimeFormat = null)
        {
            return obj.ToJson(new JsonOptions(formatting, ignoreNull, dateTimeZoneHandling, useCamelCase, dateTimeFormat));
        }

        public static string ToJsonCamel(this object obj,
            Formatting formatting = Formatting.None,
            bool ignoreNull = false,
            DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Local,
            string dateTimeFormat = null)
        {
            return obj.ToJson(new JsonOptions(formatting, ignoreNull, dateTimeZoneHandling, true, dateTimeFormat));
        }

        public static JToken ToJToken(this string str)
        {
            return JToken.Parse(str);
        }

        public static JObject ToJObject(this JToken token)
        {
            return token.ToObject<JObject>();
        }

        public static JArray ToJArray(this JToken token)
        {
            return token.ToObject<JArray>();
        }

        public static string ToSimpleString(this JToken obj)
        {
            return obj.ToString(Formatting.None);
        }

        public static int ToInt(this JToken token)
        {
            return token.ToObject<int>();
        }

        public static long ToLong(this JToken token)
        {
            return token.ToObject<long>();
        }

        public static T ToEnum<T>(this JToken token, T defaultVaule = default)
            where T : struct, Enum
        {
            return token.ToString().ToEnum(defaultVaule);
        }

        public static XmlDocument ToXmlNode(this JToken token, string deserializeRootElementName, bool writeArrayAttribute)
        {
            var converter = new XmlNodeConverter
            {
                DeserializeRootElementName = deserializeRootElementName,
                WriteArrayAttribute = writeArrayAttribute
            };
            return token.ToObject<XmlDocument>(JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { converter }
            }));
        }

        public static XDocument ToXNode(this JToken token, string deserializeRootElementName, bool writeArrayAttribute)
        {
            var converter = new XmlNodeConverter
            {
                DeserializeRootElementName = deserializeRootElementName,
                WriteArrayAttribute = writeArrayAttribute
            };
            return token.ToObject<XDocument>(JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { converter }
            }));
        }

        public static Dictionary<string, string> ToStrDic(this JObject jObject)
        {
            var dic = new Dictionary<string, string>(jObject.Count);
            foreach (var (key, value) in jObject)
            {
                dic[key] = value.ToStringOrNull();
            }
            return dic;
        }
    }
}

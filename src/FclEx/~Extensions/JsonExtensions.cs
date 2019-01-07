using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace FclEx
{
    public static class JsonExtensions
    {
        private static readonly IContractResolver _camelResolver = new CamelCasePropertyNamesContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            }
        };

        private static readonly JsonSerializerSettings _ignoreSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializerSettings _camelSettings = new JsonSerializerSettings
        {
            ContractResolver = _camelResolver
        };

        private static readonly JsonSerializerSettings _camelIgnoreNullSettings = new JsonSerializerSettings
        {
            ContractResolver = _camelResolver,
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializer _defaultSerializer = JsonSerializer.CreateDefault();
        private static readonly JsonSerializer _camelSerializer = JsonSerializer.Create(_camelSettings);

        public static string ToJson(this object obj,
            JsonSerializerSettings settings,
            Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, formatting, settings);
        }

        public static string ToJson(this object obj, Formatting formatting = Formatting.None, bool ignoreNull = false)
        {
            return ToJson(obj, ignoreNull ? _ignoreSettings : null, formatting);
        }

        public static string ToJsonCamel(this object obj, Formatting formatting = Formatting.None, bool ignoreNull = false)
        {
            return ToJson(obj, ignoreNull ? _camelIgnoreNullSettings : _camelSettings, formatting);
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

        public static T ToEnum<T>(this JToken token, T defaultVaule = default(T))
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

        public static JToken ToJToken(this object obj, JsonSerializer jsonSerializer = null) => JToken.FromObject(obj, jsonSerializer ?? _defaultSerializer);
        public static JToken ToJTokenCamel(this object obj) => JToken.FromObject(obj, _camelSerializer);
        public static JObject ToJObject(this object obj, JsonSerializer jsonSerializer = null) => JObject.FromObject(obj, jsonSerializer ?? _defaultSerializer);
        public static JObject ToJObjectCamel(this object obj) => JObject.FromObject(obj, _camelSerializer);
        public static JArray ToJArray(this object obj, JsonSerializer jsonSerializer = null) => JArray.FromObject(obj, jsonSerializer ?? _defaultSerializer);
        public static JArray ToJArrayCamel(this object obj) => JArray.FromObject(obj, _camelSerializer);

        public static Dictionary<string, string> ToStrDic(this JObject jObject)
        {
            var dic = new Dictionary<string, string>(jObject.Count);
            foreach (var m in jObject)
            {
                dic[m.Key] = m.Value.ToStringOrNull();
            }
            return dic;
        }
    }
}

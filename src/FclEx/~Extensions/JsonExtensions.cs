using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static JsonSerializerSettings IgnoreSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        internal static JsonSerializerSettings CamelSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = CamelResolver
        };

        internal static JsonSerializerSettings CamelIgnoreNullSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = CamelResolver,
            NullValueHandling = NullValueHandling.Ignore
        };

        internal static JsonSerializer DefaultSerializer { get; } = JsonSerializer.CreateDefault();
        internal static JsonSerializer CamelSerializer { get; } = JsonSerializer.Create(CamelSettings);

        public static string ToJson(this object obj, JsonSerializerSettings settings, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, formatting, settings);
        }

        public static string ToJson(this object obj, Formatting formatting = Formatting.None, bool ignoreNull = false)
        {
            return ToJson(obj, ignoreNull ? IgnoreSettings : null, formatting);
        }

        public static string ToJsonCamel(this object obj, Formatting formatting = Formatting.None, bool ignoreNull = false)
        {
            return ToJson(obj, ignoreNull ? CamelIgnoreNullSettings : CamelSettings, formatting);
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Extensions
{
    public static class JsonExtensions
    {
        private static readonly ConcurrentDictionary<JsonOptions, JsonSerializer> _serializers
            = new ConcurrentDictionary<JsonOptions, JsonSerializer>();

        internal static JsonSerializer CamelSerializer { get; } = GetSerializer(new JsonOptions(useCamelCase: true));

        internal static JsonSerializer GetSerializer(JsonOptions options)
        {
            return _serializers.GetOrAdd(options, k => JsonSerializer.Create(FclEx.JsonExtensions.GetSettings(k)));
        }

        public static JToken SerializeToJToken(this object obj, JsonOptions options = default)
            => JToken.FromObject(obj, GetSerializer(options));

        public static JToken SerializeToJTokenCamel(this object obj)
            => JToken.FromObject(obj, CamelSerializer);

        public static JObject SerializeToJObject(this object obj, JsonOptions options = default)
            => JObject.FromObject(obj, GetSerializer(options));

        public static JObject SerializeToJObjectCamel(this object obj)
            => JObject.FromObject(obj, CamelSerializer);

        public static JArray SerializeToJArray(this object obj, JsonOptions options = default)
            => JArray.FromObject(obj, GetSerializer(options));

        public static JArray SerializeToJArrayCamel(this object obj)
            => JArray.FromObject(obj, CamelSerializer);

        public static bool IsPossibleJson(this string data)
        {
            return (!data.IsNullOrEmpty() && (data.First() == '{' && data.Last() == '}'
                                              || data.First() == '[' && data.Last() == ']'));
        }

        public static bool IsPossibleJObject(this string data)
        {
            return (!data.IsNullOrEmpty() && (data.First() == '{' && data.Last() == '}'));
        }

        public static bool IsPossibleJArray(this string data)
        {
            return (!data.IsNullOrEmpty() && (data.First() == '[' && data.Last() == ']'));
        }

        public static bool TryToJToken(this string str, out JToken token)
        {
            token = null;
            if (str.IsPossibleJson())
            {
                var r = OperateResult.Excute(() => JToken.Parse(str));
                if (r.Successful)
                {
                    token = r.Result;
                    return true;
                }
            }
            return false;
        }

        public static bool TryToJObject(this string str, out JObject token)
        {
            token = null;
            if (str.IsPossibleJson())
            {
                var r = OperateResult.Excute(() => JObject.Parse(str));
                if (r.Successful)
                {
                    token = r.Result;
                    return true;
                }
            }
            return false;
        }

        public static bool TryToJArray(this string str, out JArray token)
        {
            token = null;
            if (str.IsPossibleJson())
            {
                var r = OperateResult.Excute(() => JArray.Parse(str));
                if (r.Successful)
                {
                    token = r.Result;
                    return true;
                }
            }
            return false;
        }
    }
}

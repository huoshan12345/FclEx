using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static FclEx.JsonExtensions;

namespace FclEx.Extensions
{
    public static class JsonExtensions
    {
        internal static JsonSerializer DefaultSerializer { get; } = JsonSerializer.Create(GetSettings(new JsonOptions()));
        internal static JsonSerializer CamelSerializer { get; } = JsonSerializer.Create(GetSettings(new JsonOptions(useCamelCase: true)));

        public static JToken SerializeToJToken(this object obj, JsonSerializer jsonSerializer = null)
            => JToken.FromObject(obj, jsonSerializer ?? DefaultSerializer);

        public static JToken SerializeToJTokenCamel(this object obj) 
            => JToken.FromObject(obj, CamelSerializer);

        public static JObject SerializeToJObject(this object obj, JsonSerializer jsonSerializer = null)
            => JObject.FromObject(obj, jsonSerializer ?? DefaultSerializer);

        public static JObject SerializeToJObjectCamel(this object obj) 
            => JObject.FromObject(obj, CamelSerializer);

        public static JArray SerializeToJArray(this object obj, JsonSerializer jsonSerializer = null) 
            => JArray.FromObject(obj, jsonSerializer ?? DefaultSerializer);

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

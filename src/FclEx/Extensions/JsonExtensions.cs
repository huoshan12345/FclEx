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
        public static JToken ToJToken(this object obj, JsonSerializer jsonSerializer = null)
            => JToken.FromObject(obj, jsonSerializer ?? DefaultSerializer);

        public static JToken ToJTokenCamel(this object obj) 
            => JToken.FromObject(obj, CamelSerializer);

        public static JObject ToJObject(this object obj, JsonSerializer jsonSerializer = null)
            => JObject.FromObject(obj, jsonSerializer ?? DefaultSerializer);

        public static JObject ToJObjectCamel(this object obj) 
            => JObject.FromObject(obj, CamelSerializer);

        public static JArray ToJArray(this object obj, JsonSerializer jsonSerializer = null) 
            => JArray.FromObject(obj, jsonSerializer ?? DefaultSerializer);

        public static JArray ToJArrayCamel(this object obj) 
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
                var r = ExcuteResult.Excute(() => JToken.Parse(str));
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
                var r = ExcuteResult.Excute(() => JObject.Parse(str));
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
                var r = ExcuteResult.Excute(() => JArray.Parse(str));
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

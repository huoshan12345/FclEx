using System.Collections.Concurrent;
using System.Linq;
using FclEx.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Extensions.Json
{
    public static class JsonExtensions
    {
        public static bool IsPossibleJson(this string? data)
        {
            return data.IsValid() && data!.Length >= 2
                                   && (data.First() == '{' && data.Last() == '}'
                                       || data.First() == '[' && data.Last() == ']');
        }

        public static bool IsPossibleJObject(this string? data)
        {
            return data.IsValid() && data!.Length >= 2
                                  && (data.First() == '{' && data.Last() == '}');
        }

        public static bool IsPossibleJArray(this string? data)
        {
            return data.IsValid() && data!.Length >= 2
                                  && (data.First() == '[' && data.Last() == ']');
        }

        public static bool TryToJToken(this string str, out JToken? token)
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

        public static bool TryToJObject(this string? str, out JObject? token)
        {
            token = null;
            if (str.IsPossibleJson())
            {
                var r = OperateResult.Excute(() => JObject.Parse(str!));
                if (r.Successful)
                {
                    token = r.Result;
                    return true;
                }
            }
            return false;
        }

        public static bool TryToJArray(this string str, out JArray? token)
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

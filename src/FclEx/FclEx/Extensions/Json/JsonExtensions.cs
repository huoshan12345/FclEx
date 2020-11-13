using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using FclEx.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Extensions.Json
{
    public static class JsonExtensions
    {
        public static bool IsPossibleJson([NotNullWhen(true)] this string? data)
        {
            /*
             * In JSON, values must be one of the following data types:
                a string
                a number
                an object (JSON object)
                an array
                a boolean
                null
             */

            if (data.IsValid())
            {
                if (data!.Length == 1 && data[0].IsDigit()) return true; // a single digit
                else if (data!.Length >= 2)
                {
                    if (data == "null") return true; // null
                    if (data == "true" || data == "false") return true; // a boolean

                    var (first, last) = (data.First(), data.Last());
                    if (first == '{' && last == '}') return true; // an object
                    if (first == '[' && last == ']') return true; // an array
                    if (first == '"' && last == '"') return true; // a string

                    if (first.IsDigit() && last.IsDigit()) return true; // a positive number
                    if (data.Length >= 3 && first == '-' && data[1].IsDigit() && last.IsDigit()) return true; // a negative number
                }

            }
            return false;
        }

        public static bool IsPossibleJObject([NotNullWhen(true)] this string? data)
        {
            return data.IsValid() && data!.Length >= 2
                                  && (data.First() == '{' && data.Last() == '}');
        }

        public static bool IsPossibleJArray([NotNullWhen(true)] this string? data)
        {
            return data.IsValid() && data!.Length >= 2
                                  && (data.First() == '[' && data.Last() == ']');
        }

        public static bool TryToJToken(this string str, [NotNullWhen(true)] out JToken? token)
        {
            token = null;
            if (str.IsPossibleJson())
            {
                var r = OperateResult.Excute(() => JToken.Parse(str));
                if (r.Successful)
                {
                    token = r.Result!;
                    return true;
                }
            }
            return false;
        }

        public static bool TryToJObject(this string? str, [NotNullWhen(true)] out JObject? token)
        {
            token = null;
            if (str.IsPossibleJObject())
            {
                var r = OperateResult.Excute(() => JObject.Parse(str!));
                if (r.Successful)
                {
                    token = r.Result!;
                    return true;
                }
            }
            return false;
        }

        public static bool TryToJArray(this string str, [NotNullWhen(true)] out JArray? token)
        {
            token = null;
            if (str.IsPossibleJArray())
            {
                var r = OperateResult.Excute(() => JArray.Parse(str));
                if (r.Successful)
                {
                    token = r.Result!;
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Converters;
using Formatting = Newtonsoft.Json.Formatting;

namespace FclEx.Extensions;

public static class JsonExtensions
{
    public static string ToNewtonsoftJson(this object? obj, JsonOptions options)
    {
        var settings = JsonHelper.GetSettings(options);
        return JsonConvert.SerializeObject(obj, settings);
    }

    public static string ToNewtonsoftJson(this object? obj,
        Formatting formatting = Formatting.None,
        bool ignoreNull = false,
        DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Utc,
        bool useCamelCase = false,
        string? dateTimeFormat = null)
    {
        return obj.ToNewtonsoftJson(new JsonOptions(formatting, ignoreNull, dateTimeZoneHandling, useCamelCase, dateTimeFormat));
    }

    public static string ToJsonCamel(this object? obj,
        Formatting formatting = Formatting.None,
        bool ignoreNull = false,
        DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Utc,
        string? dateTimeFormat = null)
    {
        return obj.ToNewtonsoftJson(new JsonOptions(formatting, ignoreNull, dateTimeZoneHandling, true, dateTimeFormat));
    }

    public static JToken ToJToken(this string str)
    {
        return JToken.Parse(str);
    }

    public static JObject? ToJObject(this JToken token)
    {
        return token.ToObject<JObject>();
    }

    public static JArray? ToJArray(this JToken token)
    {
        return token.ToObject<JArray>();
    }

    public static int ToInt(this JToken token)
    {
        return token.ToObject<int>();
    }

    public static long ToLong(this JToken token)
    {
        return token.ToObject<long>();
    }

    public static T ToEnum<T>(this JToken token, T defaultVaule = default) where T : struct, Enum
    {
        return token.Type switch
        {
            JTokenType.Null => defaultVaule,
            JTokenType.Integer => token.ToObject<long>().CastTo<T>(),
            JTokenType.String => token.ToString().ToEnum(defaultVaule),
            _ => defaultVaule
        };
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
        }))!;
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
        }))!;
    }

    public static Dictionary<string, string?> ToStrDic(this JObject jObject)
    {
        var dic = new Dictionary<string, string?>(jObject.Count);
        foreach (var (key, value) in jObject)
        {
            dic[key] = value?.ToString();
        }
        return dic;
    }

    public static bool Equals(this JToken? token, string value, StringComparison comparison = StringComparison.Ordinal)
    {
        return token is JValue jValue && string.Equals(jValue.Value as string, value, comparison);
    }

    public static bool IsPossibleJObject([NotNullWhen(true)] this string? data)
    {
        return data.IsNotEmpty() && data.Length >= 2
                              && (data.First() == '{' && data.Last() == '}');
    }

    public static bool IsPossibleJArray([NotNullWhen(true)] this string? data)
    {
        return data.IsNotEmpty() && data.Length >= 2
                              && (data.First() == '[' && data.Last() == ']');
    }

    public static bool TryToJToken(this string str, [NotNullWhen(true)] out JToken? token)
    {
        token = null;
        if (str.IsPossibleJson())
        {
            var r = Operate.Execute(() => JToken.Parse(str));
            if (r.Success)
            {
                token = r.Value!;
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
            var r = Operate.Execute(() => JObject.Parse(str));
            if (r.Success)
            {
                token = r.Value!;
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
            var r = Operate.Execute(() => JArray.Parse(str));
            if (r.Success)
            {
                token = r.Value!;
                return true;
            }
        }
        return false;
    }

    public static string ToNewtonsoftJson(this XNode xml, Formatting formatting = Formatting.None, bool omitRootObject = false)
    {
        return JsonConvert.SerializeXNode(xml, formatting, omitRootObject);
    }

    public static string ToNewtonsoftJson(this XmlNode xml, Formatting formatting = Formatting.None, bool omitRootObject = false)
    {
        return JsonConvert.SerializeXmlNode(xml, formatting, omitRootObject);
    }
}
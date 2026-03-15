// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0051

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xunit;

public static class XunitSerializationInfoExtensions
{
    extension(IXunitSerializationInfo info)
    {
        public void AddValue(FieldInfo member, object? value)
        {
            var dataMember = new DataMemberInfo(member);
            info.AddValue(dataMember, value);
        }

        public void AddValue(DataMemberInfo member, object? value)
        {
            if (value == null)
                return;

            var name = member.Name;
            var type = member.DataMemberType;

#if !FCLEX_XUNIT_V3
            info.AddValue(name, value, type);
#else
            if (SerializationHelper.Instance.IsSerializable(value, type))
            {
                info.AddValue(name, value, type);
                return;
            }

            if (member.IsDefined<JsonIgnoreAttribute>())
                return;

            var options = JsonHelper.GetOptions();
            if (member.TryGetAttribute<JsonConverterAttribute>(false, out var converterAttribute))
            {
                var converter = converterAttribute.ConverterType is { } converterType
                    ? (JsonConverter?)Activator.CreateInstance(converterType)
                    : converterAttribute.CreateConverter(type);

                if (converter is not null)
                {
                    options = JsonHelper.CreateOptions();
                    options.Converters.Add(converter);
                    options.MakeReadOnly(true);
                }
            }

            var json = value.ToJson(options);
            info.AddValue($"{name}__json", json, typeof(string));
            info.AddValue($"{name}__type", type.AssemblyQualifiedName);
#endif
        }

        public object? GetValue(FieldInfo member)
        {
            var dataMember = new DataMemberInfo(member);
            return info.GetValue(dataMember);
        }

        public object? GetValue(DataMemberInfo member)
        {
            var name = member.Name;
            var type = member.DataMemberType;

#if !FCLEX_XUNIT_V3
            return info.GetValue(name, type);
#else
            var value = info.GetValue(name);

            // ReSharper disable once InvertIf
            if (value is null)
            {
                if (info.GetValue($"{name}__json") is string json)
                {
                    value = json.FromJson(type);
                }
            }

            return value;
#endif
        }
    }
}


public class ObjectConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var l))
                    return l;
                return reader.GetDouble();

            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.Null:
                return null;

            default:
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                return doc.RootElement.Clone();
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0051

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xunit;

public static class XunitSerializationInfoExtensions
{
    private static readonly Type[] _arrayInterfaceTypes = typeof(int[]).GetInterfaces()
        .Select(m => m.IsGenericType ? m.GetGenericTypeDefinition() : m)
        .ToArray();

    private static readonly MethodInfo _items = typeof(ListExtensions).GetRequiredMethod(nameof(ListExtensions.Items));
    private static readonly MethodInfo _toList = typeof(Enumerable).GetRequiredMethod(nameof(Enumerable.ToList));

    extension(IXunitSerializationInfo info)
    {
        public void AddValueEx(string name, object? value, Type type)
        {
            if (value == null)
                return;

#if !FCLEX_XUNIT_V3
            info.AddValue(name, value, type);
#else
            if (SerializationHelper.Instance.IsSerializable(value, type))
            {
                info.AddValue(name, value, type);
                return;
            }

            var json = value.ToJson();
            info.AddValue($"{name}__json", json, typeof(string));
            info.AddValue($"{name}__type", type.AssemblyQualifiedName);
#endif
        }

        public object? GetValueEx(string name, Type type)
        {
#if !FCLEX_XUNIT_V3
            return info.GetValue(name, type);
#else
            var value = info.GetValue(name);

            // ReSharper disable once InvertIf
            if (value is null)
            {
                if (info.GetValue($"{name}__json") is string json)
                {
                    var options = JsonHelper.CreateOptions();
                    options.Converters.Add(new ObjectConverter());
                    value = json.FromJson(type, options);
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

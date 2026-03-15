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

            if (member.MemberInfo is FieldInfo field && field.TryGetAutoProperty(out var property))
            {

            }

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
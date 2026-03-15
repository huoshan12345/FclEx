// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0051

namespace Xunit;

public static class XunitSerializationInfoExtensions
{
#if FCLEX_XUNIT_V3
    private static bool TryGetJsonSerializerOptions(DataMemberInfo dataMember, [NotNullWhen(true)] out JsonSerializerOptions? options)
    {
        options = null;

        var (filed, property) = dataMember.GetFieldPropertyPair();
        var members = new List<MemberInfo>();
        members.AddIfNotNull(filed);
        members.AddIfNotNull(property);

        if (members.Any(m => m.IsDefined<JsonIgnoreAttribute>()))
            return false;

        var converterAttribute = members
                           .Select(m => m.GetCustomAttribute<JsonConverterAttribute>(false))
                           .FirstOrDefault(a => a is not null);

        options = JsonHelper.GetOptions();

        if (converterAttribute is null)
            return true;

        var type = dataMember.DataMemberType;
        var converter = converterAttribute.ConverterType is { } converterType
            ? (JsonConverter?)Activator.CreateInstance(converterType)
            : converterAttribute.CreateConverter(type);

        if (converter is null)
            return true;

        options = JsonHelper.CreateOptions();
        options.Converters.Insert(0, converter);
        options.MakeReadOnly(true);

        return true;
    }
#endif

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

            if (TryGetJsonSerializerOptions(member, out var options) == false)
                return;

            var json = value.ToJson(type, options);

            if (json.IsNullOrEmpty())
                return;

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
                if (info.GetValue($"{name}__json") is not string json)
                    return null;

                if (json.IsNullOrEmpty())
                    return null;

                if (TryGetJsonSerializerOptions(member, out var options) == false)
                    return null;

                value = json.FromJson(type, options);
            }

            return value;
#endif
        }
    }
}
// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0051

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

            var (filed, property) = member.GetFieldPropertyPair();
            var members = new List<MemberInfo>();
            members.AddIfNotNull(filed);
            members.AddIfNotNull(property);

            if (members.Any(m => m.IsDefined<JsonIgnoreAttribute>()))
                return;

            var options = GetOptions(type, members);
            var json = value.ToJson(options);
            info.AddValue($"{name}__json", json, typeof(string));
            info.AddValue($"{name}__type", type.AssemblyQualifiedName);


            static JsonSerializerOptions GetOptions(Type memberType, List<MemberInfo> members)
            {
                var converterAttribute = members
                    .Select(m => m.GetCustomAttribute<JsonConverterAttribute>(false))
                    .FirstOrDefault(a => a is not null);

                var options = JsonHelper.GetOptions();

                if (converterAttribute is null)
                    return options;

                var converter = converterAttribute.ConverterType is { } converterType
                    ? (JsonConverter?)Activator.CreateInstance(converterType)
                    : converterAttribute.CreateConverter(memberType);

                if (converter is null)
                    return options;

                options = JsonHelper.CreateOptions();
                options.Converters.Add(converter);
                options.MakeReadOnly(true);

                return options;
            }
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
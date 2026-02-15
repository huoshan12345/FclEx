namespace Xunit;

public static class IXunitSerializationInfoExtensions
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
                    return json.FromJson(type);
                }
            }

            return value;
#endif
        }
    }
}

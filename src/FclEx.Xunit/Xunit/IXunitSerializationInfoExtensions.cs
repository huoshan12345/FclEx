namespace Xunit;

public static class IXunitSerializationInfoExtensions
{
    private static readonly Type[] _arrayInterfaceTypes = typeof(int[]).GetInterfaces()
        .Select(m => m.IsGenericType ? m.GetGenericTypeDefinition() : m).ToArray();

    private static readonly MethodInfo _asArray = typeof(ListExtensions).GetRequiredMethod(nameof(ListExtensions.AsArray));

    extension(IXunitSerializationInfo info)
    {
        public void AddValueEx(string name, object? value, Type type)
        {
#if !FCLEX_XUNIT_V3
            info.AddValue(name, value, type);
#else
            if (value == null)
                return;

            if (SerializationHelper.Instance.IsSerializable(value, type))
            {
                info.AddValue(name, value, type);
                return;
            }

            // convert value to array
            if (_arrayInterfaceTypes.Contains(type) && value is IEnumerable enumerable && type.EnumerableType() is { } elementType)
            {
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = Activator.CreateInstance(listType, enumerable);
                var array = _asArray.MakeGenericMethod(elementType).Invoke(null, [list]);

                info.AddValue(name, array, array?.GetType());
            }
#endif
        }

        public object? GetValueEx(string name, Type type)
        {
#if !FCLEX_XUNIT_V3
            return info.GetValue(name, type);
#else
            return info.GetValue(name);
#endif
        }
    }
}

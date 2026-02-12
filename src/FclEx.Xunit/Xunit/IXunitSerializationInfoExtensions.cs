namespace Xunit;

public static class IXunitSerializationInfoExtensions
{
    private static readonly Type[] _arrayInterfaceTypes = typeof(int[]).GetInterfaces()
        .Select(m => m.IsGenericType ? m.GetGenericTypeDefinition() : m)
        .ToArray();

    private static readonly MethodInfo _asArray = typeof(ListExtensions).GetRequiredMethod(nameof(ListExtensions.AsArray));
    private static readonly MethodInfo _toList = typeof(Enumerable).GetRequiredMethod(nameof(Enumerable.ToList));

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

            var genericDefOrSelf = type.IsGenericType
                ? type.GetGenericTypeDefinition()
                : type;

            var elementType = type.EnumerableType();

            // convert enumerable value to array
            if (_arrayInterfaceTypes.Contains(genericDefOrSelf) && value is IEnumerable enumerable && elementType is not null)
            {
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = Activator.CreateInstance(listType, enumerable);
                var array = _asArray.MakeGenericMethod(elementType).Invoke(null, [list]);
                info.AddValue(name, array, array?.GetType());
                return;
            }

            // convert list value to array
            if (genericDefOrSelf == typeof(List<>) && elementType is not null)
            {
                var array = _asArray.MakeGenericMethod(elementType).Invoke(null, [value]);
                info.AddValue(name, array, array?.GetType());
                return;
            }

            info.AddValue(name, value, type);
#endif
        }

        public object? GetValueEx(string name, Type type)
        {
#if !FCLEX_XUNIT_V3
            return info.GetValue(name, type);
#else

            var genericDefOrSelf = type.IsGenericType
                ? type.GetGenericTypeDefinition()
                : type;

            var elementType = type.EnumerableType();

            if (genericDefOrSelf == typeof(List<>) && elementType is not null)
            {
                var value = info.GetValue(name);
                if (value is null)
                    return null;

                var list = _toList.MakeGenericMethod(elementType).Invoke(null, [value]);
                return list;
            }

            return info.GetValue(name);
#endif
        }
    }
}

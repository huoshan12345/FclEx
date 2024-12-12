using static FclEx.BindingAttributes;

namespace FclEx.Extensions;

partial class TypeExtensions
{
    private static T? GetMember<T>(this Type type, Func<Type, T?> selector, bool searchBaseTypes) where T : MemberInfo
    {
        var t = type;
        while (t is not null)
        {
            var member = selector(t);
            if (member is not null)
                return member;

            if (searchBaseTypes == false)
                return member;

            t = t.BaseType;
        }
        return null;
    }

    public static FieldInfo? GetField(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember<FieldInfo>(m => m.GetField(name, AllDeclared), searchBaseTypes);
    }

    public static FieldInfo GetRequiredField(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetField(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find field '{name}' in type '{type.FullName}'");
    }

    public static FieldInfo GetAutoPropertyBackingField(this Type type, string propertyName, bool searchBaseTypes)
    {
        return type.GetField($"<{propertyName}>k__BackingField", searchBaseTypes)
               ?? throw new InvalidOperationException($"Cannot find backing field for property '{propertyName}' in type '{type.FullName}'"); ;
    }

    public static PropertyInfo? GetProperty(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetProperty(name, AllDeclared), searchBaseTypes);
    }

    public static PropertyInfo GetRequiredProperty(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetProperty(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find property '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo? GetMethod(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetMethod(name, AllDeclared), searchBaseTypes);
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetMethod(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find method '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo? GetMethod(this Type type, string name, int genericArgumentCount, Type[] paramTypes, bool searchBaseTypes = false)
    {
        return type.GetMember(t =>
        {
            return t.GetMethods(AllDeclared)
                .Where(m => m.Name == name)
                .Select(m => (Method: m, Params: m.GetParameters(), Args: m.GetGenericArguments()))
                .Where(x => x.Args.Length == genericArgumentCount
                            && x.Params.Length == paramTypes.Length
                            && x.Params.Select(m => m.ParameterType).SequenceEqual(paramTypes))
                .Select(x => x.Method)
                .FirstOrDefault();
        }, searchBaseTypes);
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name, int genericArgumentCount, Type[] paramTypes, bool searchBaseTypes)
    {
        return type.GetMethod(name, genericArgumentCount, paramTypes, searchBaseTypes)
               ?? throw new InvalidOperationException($"Cannot find method '{name}<`{genericArgumentCount}>({paramTypes.Select(m => m.Name).JoinWith(", ")})' in type '{type.FullName}'");
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name, int genericArgumentCount, params Type[] paramTypes)
    {
        return type.GetRequiredMethod(name, genericArgumentCount, paramTypes, false);
    }

    public static ConstructorInfo GetRequiredConstructor(this Type type, params Type[] types)
    {
        return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, types)
               ?? throw new InvalidOperationException($"Cannot find constructor({types.Select(m => m.Name).JoinWith(", ")}) in type '{type.FullName}'");
    }

    private static readonly ConcurrentDictionary<Type, IReadOnlyList<FieldInfo>> _cache = new();

    /// <summary>
    /// Retrieves all instance fields of a specified type, including fields declared in its base types.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> for which to retrieve all instance fields.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{FieldInfo}"/> containing all instance fields of the type and its base types.
    /// </returns>
    /// <remarks>
    /// - This method caches the results for each type to optimize repeated calls.
    /// - It traverses the inheritance hierarchy, starting from the given type and including all base types, 
    ///   to gather all instance fields.
    /// </remarks>
    public static IReadOnlyList<FieldInfo> GetAllInstanceFields(this Type type)
    {
        return _cache.GetOrAdd(type, Impl);

        static IReadOnlyList<FieldInfo> Impl(Type type)
        {
            var list = new List<FieldInfo>();
            var p = type;
            while (p is not null)
            {
                var fields = p.GetFields(AllDeclaredInstance);
                list.AddRange(fields);
                p = p.BaseType;
            }
            return list.ToReadOnlyList();
        }
    }

    public static IReadOnlyCollection<DataMemberInfo> GetDataMembers(this Type type) => ReflectionHelper.GetDataMembers(type).Values;

    public static DataMemberInfo? GetDataMember(this Type type, string name) => ReflectionHelper.GetDataMembers(type).Get(name);

    public static DataMemberInfo GetRequiredDataMember(this Type type, string name)
    {
        return type.GetDataMember(name) ?? throw new InvalidOperationException($"Cannot find field or property '{name}' in type '{type.FullName}'");
    }
}
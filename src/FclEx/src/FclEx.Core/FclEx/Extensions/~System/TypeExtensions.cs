using static FclEx.BindingAttributes;

namespace FclEx.Extensions;

public static partial class TypeExtensions
{
    public static object CreateObject(this Type type, params object?[] args)
    {
        FclEx.Check.NotNull(type);

        if (args.IsNullOrEmpty())
            return Activator.CreateInstance(type)!;

        var argsType = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
        var ctor = type.GetConstructors().FirstOrDefault(m => m.ArgumentListMatches(argsType));
        if (ctor != null)
        {
            var paras = ctor.GetParameters();
            if (paras.Length > args.Length)
            {
                args = args.Concat(paras.Skip(args.Length).Select(m => m.RawDefaultValue)).ToArray();
            }
            return ctor.Invoke(args);
        }

        throw new MissingMethodException();
    }

    public static T CreateObject<T>(this Type type, params object?[] args)
    {
        return type.CreateObject(args).CastTo<T>();
    }

    public static MethodInfo GetMethod(this Type type, string methodName, int pParametersCount = 0, int pGenericArgumentsCount = 0)
    {
        FclEx.Check.NotNull(type);

        return type.GetMethods()
            .Where(m => m.Name == methodName)
            .Select(m => new
            {
                Method = m,
                Params = m.GetParameters(),
                Args = m.GetGenericArguments()
            })
            .Where(x => x.Params.Length == pParametersCount
                        && x.Args.Length == pGenericArgumentsCount
            ).Select(x => x.Method)
            .First();
    }

    public static bool SequenceAssignableFrom(this IEnumerable<Type> first, IEnumerable<Type> second)
    {
        var comparer = EqualityComparer<Type>.Default;
        using var e1 = first.GetEnumerator();
        using var e2 = second.GetEnumerator();
        while (e1.MoveNext())
        {
            if (!e2.MoveNext()) return false;
            else if (!(comparer.Equals(e1.Current, e2.Current) || e1.Current.IsAssignableFrom(e2.Current)))
                return false;
        }
        if (e2.MoveNext())
            return false;

        return true;
    }

    public static bool IsInheritedFromGenericType(this Type type, Type genericType)
    {
        return GetGenericInterface(type, genericType) != null;
    }

    public static Type? GetGenericInterface(this Type type, Type genericType)
    {
        return type.GetInterfaces().FirstOrDefault(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == genericType);
    }

    public static bool IsSubclassOfRawGeneric(this Type? toCheck, Type generic)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }

    public static IEnumerable<MemberInfo> EnumeratePropertyOrField(this Type type, BindingFlags bindingFlags)
    {
        return type.GetFields(bindingFlags).Cast<MemberInfo>()
            .Concat(type.GetProperties(bindingFlags));
    }

    public static IReadOnlyCollection<DataMemberInfo> GetDataMembers(this Type type) => ReflectionHelper.GetDataMembers(type).Values;

    public static DataMemberInfo? GetDataMember(this Type type, string name) => ReflectionHelper.GetDataMembers(type).Get(name);

    public static T? GetDataMemberValue<T>(this Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Static
                                   | BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.GetField | BindingFlags.GetProperty;
        return type.InvokeMember(name, flags, null, null, null).CastTo<T?>();
    }

    public static T? GetDataMemberValue<T>(this Type type, string name, object? obj)
    {
        const BindingFlags flags = BindingFlags.Instance
                                   | BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.GetField | BindingFlags.GetProperty;
        return type.InvokeMember(name, flags, null, obj, null).CastTo<T>();
    }

    public static bool IsDynamic(this Type type)
    {
        return type.IsDefined<DynamicAttribute>(true);
    }

    public static FieldInfo GetRequiredField(this Type type, string name)
    {
        return type.GetField(name, AllDeclared) ?? throw new InvalidOperationException($"Cannot find field '{name}' in type '{type.FullName}'");
    }

    public static FieldInfo GetAutoPropertyBackingField(this Type type, string propertyName)
    {
        return type.GetField($"<{propertyName}>k__BackingField", AllDeclared)
               ?? throw new InvalidOperationException($"Cannot find backing field for property '{propertyName}' in type '{type.FullName}'"); ;
    }

    public static PropertyInfo GetRequiredProperty(this Type type, string name)
    {
        return type.GetProperty(name, AllDeclared) ?? throw new InvalidOperationException($"Cannot find property '{name}' in type '{type.FullName}'");
    }

    public static bool TryGetProperty(this Type type, string name, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = type.GetProperty(name, AllDeclared);
        return property != null;
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name)
    {
        return type.GetMethod(name, AllDeclared) ?? throw new InvalidOperationException($"Cannot find method '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name, int genericArgumentCount, params Type[] paramTypes)
    {
        return type.GetMethods(AllDeclared)
            .Where(m => m.Name == name)
            .Select(m => (Method: m, Params: m.GetParameters(), Args: m.GetGenericArguments()))
            .Where(x => x.Args.Length == genericArgumentCount
                        && x.Params.Length == paramTypes.Length
                        && x.Params.Select(m => m.ParameterType).SequenceEqual(paramTypes))
            .Select(x => x.Method)
            .FirstOrDefault() ?? throw new InvalidOperationException($"Cannot find method '{name}<`{genericArgumentCount}>({paramTypes.Select(m => m.Name).JoinWith(", ")})' in type '{type.FullName}'");
    }

    public static ConstructorInfo GetRequiredConstructor(this Type type, params Type[] types)
    {
        return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, types)
               ?? throw new InvalidOperationException($"Cannot find constructor({types.Select(m => m.Name).JoinWith(", ")}) in type '{type.FullName}'");
    }

    public static FieldInfo[] GetAllInstanceFields(this Type type)
    {
        var list = new List<FieldInfo>();
        var p = type;
        while (p is not null)
        {
            var fields = p.GetFields(AllDeclaredInstance);
            list.AddRange(fields);
            p = p.BaseType;
        }
        return list.AsSpan().ToArray();
    }

#if NETSTANDARD2_0
    public static ConstructorInfo? GetConstructor(this Type type, BindingFlags bindingAttr, Type[] types)
    {
        return type.GetConstructor(bindingAttr, null, types, null);
    }

    public static bool IsAssignableTo(this Type type, [NotNullWhen(true)] Type? targetType) => targetType?.IsAssignableFrom(type) ?? false;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeCode GetTypeCode(this Type type)
    {
        return Type.GetTypeCode(type);
    }
}
namespace FclEx.Extensions;

public static partial class TypeExtensions
{
    public static object? DefaultValueByExp(this Type type)
    {
        Check.NotNull(type);

        // We want an Func<object> which returns the default.
        // Create that expression here.
        var e = Expression.Lambda<Func<object?>>(
            // Have to convert to object.
            Expression.Convert(
                // The default value, always get what the *code* tells us.
                Expression.Default(type), typeof(object)
            )
        );

        // Compile and return the value.
        return e.Compile()();
    }

    public static object CreateObject(this Type type, params object?[] args)
    {
        Check.NotNull(type);

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
        Check.NotNull(type);

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
        return type.InvokeMember(name, flags, null, null, null).CastTo<T>();
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

    public const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static FieldInfo GetRequiredField(this Type type, string name)
    {
        return type.GetField(name, MemberBindingFlags) ?? throw new InvalidOperationException($"Cannot find field '{name}' in type '{type.FullName}'");
    }


    public static FieldInfo GetBackingField(this Type type, string name)
    {
        return type.GetField($"<{name}>k__BackingField", MemberBindingFlags)
               ?? throw new InvalidOperationException($"Cannot find backing field for property '{name}' in type '{type.FullName}'"); ;
    }

    public static PropertyInfo GetRequiredProperty(this Type type, string name)
    {
        return type.GetProperty(name, MemberBindingFlags) ?? throw new InvalidOperationException($"Cannot find property '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name)
    {
        return type.GetMethod(name, MemberBindingFlags) ?? throw new InvalidOperationException($"Cannot find method '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name, int genericArgumentCount, params Type[] paramTypes)
    {
        return type.GetMethods(MemberBindingFlags)
            .Where(m => m.Name == name)
            .Select(m => (Method: m, Params: m.GetParameters(), Args: m.GetGenericArguments()))
            .Where(x => x.Args.Length == genericArgumentCount
                        && x.Params.Length == paramTypes.Length
                        && x.Params.Select(m => m.ParameterType).SequenceEqual(paramTypes))
            .Select(x => x.Method)
            .FirstOrDefault() ?? throw new InvalidOperationException($"Cannot find method '{name}<`{genericArgumentCount}>({paramTypes.Select(m => m.Name).JoinWith(", ")})' in type '{type.FullName}'");
    }

#if NETSTANDARD2_0
    public static ConstructorInfo? GetConstructor(this Type type, BindingFlags bindingAttr, Type[] types)
    {
        return type.GetConstructor(bindingAttr, null, types, null);
    }
#endif

    public static ConstructorInfo GetRequiredConstructor(this Type type, params Type[] types)
    {
        return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, types)
               ?? throw new InvalidOperationException($"Cannot find constructor({types.Select(m => m.Name).JoinWith(", ")}) in type '{type.FullName}'");
    }
}
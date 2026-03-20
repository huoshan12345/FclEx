using static System.Reflection.BindingAttributes;

namespace FclEx.Extensions;

partial class TypeExtensions
{
    private static T? GetMember<T>(this Type type, Func<Type, T?> selector, bool searchBaseTypes) where T : MemberInfo
    {
        var t = type;
        while (t is not null)
        {
            var member = selector(t);

            if (member is not null || searchBaseTypes == false)
                return member;

            t = t.BaseType;
        }
        return null;
    }

    public static FieldInfo? GetField(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetField(name, Declared), searchBaseTypes);
    }

    public static FieldInfo GetRequiredField(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetField(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find field '{name}' in type '{type.FullName}'");
    }

    public static FieldInfo GetAutoPropertyBackingField(this Type type, string propertyName, bool searchBaseTypes = false)
    {
        var name = ReflectionHelper.GetAutoBackingFieldName(propertyName);
        return type.GetField(name, searchBaseTypes)
               ?? throw new InvalidOperationException($"Cannot find backing field for property '{propertyName}' in type '{type.FullName}'");
    }

    public static PropertyInfo? GetProperty(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetProperty(name, Declared), searchBaseTypes);
    }

    public static PropertyInfo GetRequiredProperty(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetProperty(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find property '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo? GetMethod(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetMethod(name, Declared), searchBaseTypes);
    }

    public static MethodInfo GetRequiredMethod(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetMethod(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find method '{name}' in type '{type.FullName}'");
    }

    public static MethodInfo? GetMethod(this Type type, string name, int genericArgumentCount, Type[] paramTypes, bool searchBaseTypes = false)
    {
        return type.GetMember(t =>
        {
            return t.GetMethods(Declared)
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
            return type.GetDataMembers()
                .Where(m => m is { IsField: true, IsStatic: false })
                .Select(m => m.MemberInfo)
                .OfType<FieldInfo>()
                .ToReadOnlyList();
        }
    }

    public static IReadOnlyList<DataMemberInfo> GetDataMembers(this Type type) => ReflectionHelper.GetDataMembers(type);

    public static IEnumerable<DataMemberInfo> GetDataMembers(this Type type, DataMemberFlags flags)
    {
        // Must choose Declared or Inherited
        if ((flags & (DataMemberFlags.Declared | DataMemberFlags.Inherited)) == 0)
            yield break;

        // Must choose Instance or Static
        if ((flags & (DataMemberFlags.Instance | DataMemberFlags.Static)) == 0)
            yield break;

        // Must choose Public or NonPublic
        if ((flags & (DataMemberFlags.Public | DataMemberFlags.NonPublic)) == 0)
            yield break;

        // Must choose Field or Property
        if ((flags & (DataMemberFlags.Field | DataMemberFlags.Property)) == 0)
            yield break;

        // Must choose CanRead or CanWrite
        if ((flags & (DataMemberFlags.CanRead | DataMemberFlags.CanWrite)) == 0)
            yield break;

        var allowUnsafeWrite = (flags & DataMemberFlags.UnsafeWrite) != 0;

        foreach (var member in type.GetDataMembers())
        {
            // Declared / Inherited filter
            if (member.DeclaringType == type)
            {
                if ((flags & DataMemberFlags.Declared) == 0)
                    continue;
            }
            else
            {
                if ((flags & DataMemberFlags.Inherited) == 0)
                    continue;
            }

            // Instance / Static filter
            if (member.IsStatic)
            {
                if ((flags & DataMemberFlags.Static) == 0)
                    continue;
            }
            else
            {
                if ((flags & DataMemberFlags.Instance) == 0)
                    continue;
            }

            // Public / NonPublic filter
            if (member.HasPublicGetter || member.HasPublicSetter)
            {
                if ((flags & DataMemberFlags.Public) == 0)
                    continue;
            }
            else
            {
                if ((flags & DataMemberFlags.NonPublic) == 0)
                    continue;
            }

            // Field / Property filter
            if (member.MemberInfo is FieldInfo field)
            {
                if ((flags & DataMemberFlags.Field) == 0)
                    continue;

                if (field.IsAutoPropertyBackingField()
                    && (flags & DataMemberFlags.AutoPropertyBackingField) == 0)
                {
                    continue;
                }
            }
            else
            {
                if ((flags & DataMemberFlags.Property) == 0)
                    continue;

                if (member.IsIndexer
                    && (flags & DataMemberFlags.Indexer) == 0)
                {
                    continue;
                }
            }

            // Read filter
            if ((flags & DataMemberFlags.CanRead) != 0
                && !member.CanRead)
            {
                continue;
            }

            // Write filter
            if ((flags & DataMemberFlags.CanWrite) != 0)
            {
                if (member is { CanWrite: true, IsInitOnly: false })
                {
                    // writable safely OK
                }
                else if (allowUnsafeWrite && member.IsInitOnly)
                {
                    // readonly field / init property
                }
                else
                {
                    continue;
                }
            }

            yield return member;
        }
    }

    public static DataMemberInfo? GetDataMember(this Type type, string name) => ReflectionHelper.GetDataMembers(type).FirstOrDefault(m => m.Name == name);

    public static DataMemberInfo GetRequiredDataMember(this Type type, string name)
    {
        return type.GetDataMember(name) ?? throw new InvalidOperationException($"Cannot find field or property '{name}' in type '{type.FullName}'");
    }

    private const BindingFlags DefaultCtorFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Retrieves the default (parameterless) constructor of the specified type, if available.
    /// </summary>
    /// <param name="type">The type to search for a default constructor.</param>
    /// <returns>
    /// The default constructor of the specified type, or <c>null</c> if no such constructor exists.
    /// </returns>
    public static ConstructorInfo? GetDefaultConstructor(this Type type)
    {
        var ctors = type.GetConstructors(DefaultCtorFlags);
        if (ctors.Length == 0)
            return null;

        var defaultCtor = ctors.FirstOrDefault(m => m.GetParameters().Length == 0);

        return defaultCtor;
    }

    /// <summary>
    /// Retrieves the default (parameterless) constructor of the specified type.
    /// Throws an exception if the type does not have a default constructor.
    /// </summary>
    /// <param name="type">The type to search for a default constructor.</param>
    /// <returns>
    /// The default constructor of the specified type.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the type does not have any constructors or does not have a default constructor.
    /// </exception>
    public static ConstructorInfo GetRequiredDefaultConstructor(this Type type)
    {
        var ctors = type.GetConstructors(DefaultCtorFlags);
        if (ctors.Length == 0)
            throw new ArgumentException($"The type '{type.LongName()}' does not have any constructors.");

        var defaultCtor = ctors.FirstOrDefault(m => m.GetParameters().Length == 0);

        if (defaultCtor is null)
            throw new ArgumentException($"The type '{type.LongName()}' does not have a default constructor.");

        return defaultCtor;
    }

    /// <summary>
    /// Retrieves all constant fields (fields with the <c>const</c> keyword) of the specified type.
    /// </summary>
    /// <param name="type">The type to retrieve constant fields from.</param>
    /// <returns>
    /// An array of <see cref="FieldInfo"/> objects representing the constant fields of the type.
    /// </returns>
    public static FieldInfo[] GetConstants(this Type type)
    {
        var fieldInfos = type.GetDataMembers().Select(m => m.MemberInfo).OfType<FieldInfo>();

        // IsLiteral determines if its value is written at compile time and not changeable
        // IsInitOnly determines if the field can be set in the body of the constructor
        // for C# a field which is readonly keyword would have both true 
        // but a const field would have only IsLiteral equal to true
        return fieldInfos.Where(fi => fi is { IsLiteral: true, IsInitOnly: false }).ToArray();
    }
}
namespace FclEx.Extensions;

partial class TypeExtensions
{
    private static readonly ConditionalWeakTable<Type, IReadOnlyList<DataMemberInfo>> TypeDataMemberDic = new();

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

    /// <summary>
    /// Retrieves a field by name from the specified type, optionally searching base types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The field name.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the field is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching field, or <see langword="null"/> when no field is found.</returns>
    public static FieldInfo? GetField(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetField(name, BindingFlags.Declared), searchBaseTypes);
    }

    /// <summary>
    /// Retrieves a field by name and throws when it cannot be found.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The field name.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the field is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching field.</returns>
    /// <exception cref="InvalidOperationException">No matching field is found.</exception>
    public static FieldInfo GetRequiredField(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetField(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find field '{name}' in type '{type.FullName}'");
    }

    /// <summary>
    /// Gets the compiler-generated backing field for an auto-implemented property.
    /// </summary>
    /// <param name="type">The type that declares or inherits the property.</param>
    /// <param name="propertyName">The property name whose backing field should be found.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the backing field is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The auto-property backing field.</returns>
    /// <exception cref="InvalidOperationException">No matching backing field is found.</exception>
    public static FieldInfo GetAutoPropertyBackingField(this Type type, string propertyName, bool searchBaseTypes = false)
    {
        var name = ReflectionHelper.GetAutoBackingFieldName(propertyName);
        return type.GetField(name, searchBaseTypes)
               ?? throw new InvalidOperationException($"Cannot find backing field for property '{propertyName}' in type '{type.FullName}'");
    }

    /// <summary>
    /// Retrieves a property by name from the specified type, optionally searching base types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The property name.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the property is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching property, or <see langword="null"/> when no property is found.</returns>
    public static PropertyInfo? GetProperty(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetProperty(name, BindingFlags.Declared), searchBaseTypes);
    }

    /// <summary>
    /// Retrieves a property by name and throws when it cannot be found.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The property name.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the property is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching property.</returns>
    /// <exception cref="InvalidOperationException">No matching property is found.</exception>
    public static PropertyInfo GetRequiredProperty(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetProperty(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find property '{name}' in type '{type.FullName}'");
    }

    /// <summary>
    /// Retrieves a method by name from the specified type, optionally searching base types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The method name.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the method is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching method, or <see langword="null"/> when no method is found.</returns>
    /// <remarks>
    /// This method follows <see cref="Type.GetMethod(string, BindingFlags)"/> behavior for a single type in
    /// the hierarchy. If more than one declared overload has the specified name, it can throw
    /// <see cref="AmbiguousMatchException"/>.
    /// </remarks>
    public static MethodInfo? GetMethod(this Type type, string name, bool searchBaseTypes)
    {
        return type.GetMember(m => m.GetMethod(name, BindingFlags.Declared), searchBaseTypes);
    }

    /// <summary>
    /// Retrieves a method by name and throws when it cannot be found.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The method name.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the method is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching method.</returns>
    /// <exception cref="InvalidOperationException">No matching method is found.</exception>
    /// <inheritdoc cref="GetMethod(Type, string, bool)"/>
    public static MethodInfo GetRequiredMethod(this Type type, string name, bool searchBaseTypes = false)
    {
        return type.GetMethod(name, searchBaseTypes) ?? throw new InvalidOperationException($"Cannot find method '{name}' in type '{type.FullName}'");
    }

    /// <summary>
    /// Retrieves a generic or non-generic method by name, generic argument count, and exact parameter types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The method name.</param>
    /// <param name="genericArgumentCount">The number of generic method parameters.</param>
    /// <param name="paramTypes">The exact method parameter types.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the method is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching method, or <see langword="null"/> when no method is found.</returns>
    public static MethodInfo? GetMethod(this Type type, string name, int genericArgumentCount, Type[] paramTypes, bool searchBaseTypes = false)
    {
        return type.GetMember(t =>
        {
            return t.GetMethods(BindingFlags.Declared)
                .Where(m => m.Name == name)
                .Select(m => (Method: m, Params: m.GetParameters(), Args: m.GetGenericArguments()))
                .Where(x => x.Args.Length == genericArgumentCount
                            && x.Params.Length == paramTypes.Length
                            && x.Params.Select(m => m.ParameterType).SequenceEqual(paramTypes))
                .Select(x => x.Method)
                .FirstOrDefault();
        }, searchBaseTypes);
    }

    /// <summary>
    /// Retrieves a generic or non-generic method by name, generic argument count, and exact parameter types,
    /// and throws when it cannot be found.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The method name.</param>
    /// <param name="genericArgumentCount">The number of generic method parameters.</param>
    /// <param name="paramTypes">The exact method parameter types.</param>
    /// <param name="searchBaseTypes">
    /// <see langword="true"/> to continue searching base types when the method is not declared on
    /// <paramref name="type"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The matching method.</returns>
    /// <exception cref="InvalidOperationException">No matching method is found.</exception>
    public static MethodInfo GetRequiredMethod(this Type type, string name, int genericArgumentCount, Type[] paramTypes, bool searchBaseTypes)
    {
        return type.GetMethod(name, genericArgumentCount, paramTypes, searchBaseTypes)
               ?? throw new InvalidOperationException($"Cannot find method '{name}<`{genericArgumentCount}>({paramTypes.Select(m => m.Name).JoinWith(", ")})' in type '{type.FullName}'");
    }

    /// <summary>
    /// Retrieves a declared generic or non-generic method by name, generic argument count, and exact parameter types,
    /// and throws when it cannot be found.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The method name.</param>
    /// <param name="genericArgumentCount">The number of generic method parameters.</param>
    /// <param name="paramTypes">The exact method parameter types.</param>
    /// <returns>The matching method.</returns>
    /// <exception cref="InvalidOperationException">No matching method is found.</exception>
    public static MethodInfo GetRequiredMethod(this Type type, string name, int genericArgumentCount, params Type[] paramTypes)
    {
        return type.GetRequiredMethod(name, genericArgumentCount, paramTypes, false);
    }

    /// <summary>
    /// Retrieves a public or non-public instance constructor whose parameters exactly match the supplied types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="types">The constructor parameter types.</param>
    /// <returns>The matching constructor.</returns>
    /// <exception cref="InvalidOperationException">No matching constructor is found.</exception>
    public static ConstructorInfo GetRequiredConstructor(this Type type, params Type[] types)
    {
        return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, types)
               ?? throw new InvalidOperationException($"Cannot find constructor({types.Select(m => m.Name).JoinWith(", ")}) in type '{type.FullName}'");
    }

    private static readonly ConditionalWeakTable<Type, IReadOnlyList<FieldInfo>> _allInstanceFieldsCache = new();

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
        return _allInstanceFieldsCache.GetValue(type, Impl);

        static IReadOnlyList<FieldInfo> Impl(Type type)
        {
            return type.GetDataMembers()
                .Where(m => m is { IsField: true, IsStatic: false })
                .Select(m => m.MemberInfo)
                .OfType<FieldInfo>()
                .ToReadOnlyList();
        }
    }

    /// <summary>
    /// Retrieves all data members discovered for the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// Fields and properties declared by the type and its relevant base/interface hierarchy, as discovered by
    /// <see cref="ReflectionHelper.GetDataMembers(Type)"/>.
    /// </returns>
    public static IReadOnlyList<DataMemberInfo> GetDataMembers(this Type type)
    {
        return TypeDataMemberDic.GetValue(type, GetDataMembersCore);

        static IReadOnlyList<DataMemberInfo> GetDataMembersCore(Type type)
        {
            if (type.IsInterface)
            {
                var members = type.GetInterfaces()
                    .Prepend(type)
                    .Select(GetDeclaredDataMembers)
                    .SelectMany(m => m);
                return members.ToReadOnlyList();
            }

            var list = new List<DataMemberInfo>(GetVisibleDataMembers(type));

            var baseType = type.BaseType;
            while (baseType is not null)
            {
                var members = GetNotVisibleToDerivedDataMembers(baseType);
                list.AddRange(members);
                baseType = baseType.BaseType;
            }

            return list.ToReadOnlyList();
        }

        static IEnumerable<DataMemberInfo> GetDeclaredDataMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Declared)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo());
        }

        static IEnumerable<DataMemberInfo> GetVisibleDataMembers(Type type)
        {
            return type.GetMembers(BindingFlags.VisibleToDerived)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo());
        }

        static IEnumerable<DataMemberInfo> GetNotVisibleToDerivedDataMembers(Type type)
        {
            return type.GetMembers(BindingFlags.DeclaredNonPublic)
                .Where(m => m is PropertyInfo property && property.IsNotVisibleToDerived()
                            || m is FieldInfo field && field.IsNotVisibleToDerived())
                .Select(m => m.ToDataMemberInfo());
        }
    }

    /// <summary>
    /// Retrieves data members from the specified type using the supplied flag filters.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="flags">
    /// The filters to apply. At least one flag from each required group must be specified:
    /// <see cref="DataMemberFlags.Declared"/> or <see cref="DataMemberFlags.Inherited"/>,
    /// <see cref="DataMemberFlags.Instance"/> or <see cref="DataMemberFlags.Static"/>,
    /// <see cref="DataMemberFlags.Public"/> or <see cref="DataMemberFlags.NonPublic"/>,
    /// <see cref="DataMemberFlags.Field"/> or <see cref="DataMemberFlags.Property"/>, and
    /// <see cref="DataMemberFlags.CanRead"/> or <see cref="DataMemberFlags.CanWrite"/>.
    /// Missing any required group returns an empty sequence.
    /// </param>
    /// <returns>The matching data members.</returns>
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

    /// <summary>
    /// Retrieves the first data member with the specified name.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The field or property name.</param>
    /// <returns>The matching data member, or <see langword="null"/> when no member is found.</returns>
    public static DataMemberInfo? GetDataMember(this Type type, string name) => type.GetDataMembers().FirstOrDefault(m => m.Name == name);

    /// <summary>
    /// Retrieves the first data member with the specified name and throws when it cannot be found.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The field or property name.</param>
    /// <returns>The matching data member.</returns>
    /// <exception cref="InvalidOperationException">No matching field or property is found.</exception>
    public static DataMemberInfo GetRequiredDataMember(this Type type, string name)
    {
        return type.GetDataMember(name) ?? throw new InvalidOperationException($"Cannot find field or property '{name}' in type '{type.FullName}'");
    }

    /// <summary>
    /// Retrieves the public or non-public parameterless constructor of the specified type, if available.
    /// </summary>
    /// <param name="type">The type to search for a parameterless constructor.</param>
    /// <returns>
    /// The parameterless constructor of the specified type, or <see langword="null"/> if no such constructor exists.
    /// </returns>
    public static ConstructorInfo? GetParameterlessConstructor(this Type type)
    {
        var ctors = type.GetConstructors(BindingFlags.DeclaredInstance);
        if (ctors.Length == 0)
            return null;

        var parameterlessCtor = ctors.FirstOrDefault(m => m.GetParameters().Length == 0);

        return parameterlessCtor;
    }

    /// <summary>
    /// Retrieves the public or non-public parameterless constructor of the specified type.
    /// Throws an exception if the type does not have a parameterless constructor.
    /// </summary>
    /// <param name="type">The type to search for a parameterless constructor.</param>
    /// <returns>
    /// The parameterless constructor of the specified type.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the type does not have any constructors or does not have a parameterless constructor.
    /// </exception>
    public static ConstructorInfo GetRequiredParameterlessConstructor(this Type type)
    {
        var ctors = type.GetConstructors(BindingFlags.DeclaredInstance);
        if (ctors.Length == 0)
            throw new ArgumentException($"The type '{type.LongName()}' does not have any constructors.");

        var parameterlessCtor = ctors.FirstOrDefault(m => m.GetParameters().Length == 0);

        if (parameterlessCtor is null)
            throw new ArgumentException($"The type '{type.LongName()}' does not have a parameterless constructor.");

        return parameterlessCtor;
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

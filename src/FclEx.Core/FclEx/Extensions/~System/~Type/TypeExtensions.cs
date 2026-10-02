namespace FclEx.Extensions;

/// <summary>
/// Provides reflection-oriented extension methods for <see cref="Type"/>.
/// </summary>
public static partial class TypeExtensions
{
    /// <summary>
    /// Creates an instance of the specified type by invoking a public constructor whose leading parameters
    /// match the supplied arguments and whose remaining parameters are optional.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">
    /// The constructor arguments. A <see langword="null"/> argument matches any reference type parameter or
    /// <see cref="Nullable{T}"/> parameter. A null <c>params</c> array is treated as an empty argument list; pass an
    /// explicit one-element array to select a constructor with a null argument. Omitted optional parameters are supplied
    /// from their metadata default values.
    /// </param>
    /// <returns>The created object.</returns>
    /// <exception cref="MissingMethodException">No public constructor matches the supplied arguments.</exception>
    /// <exception cref="AmbiguousMatchException">More than one public constructor matches the supplied arguments.</exception>
    /// <remarks>
    /// This method is intended to behave like a default-parameter-aware version of
    /// <c>Activator.CreateInstance(type, args)</c>. It supports primitive widening conversions accepted by
    /// the default reflection binder, but it does not reorder arguments.
    /// </remarks>
    public static object CreateObject(this Type type, params object?[] args)
    {
        FclEx.Check.NotNull(type);

        args ??= [];

        if (args.Length == 0 && type.IsValueType)
            return Activator.CreateInstance(type)!;

        var matches = type.GetConstructors()
            .Where(m => ArgumentListMatches(m, args))
            .ToArray();

        if (matches.Length == 0)
            throw new MissingMethodException(type.FullName, ".ctor");

        if (matches.Length > 1)
            throw new AmbiguousMatchException($"More than one constructor of type '{type.FullName}' matches the supplied arguments.");

        var ctor = matches[0];
        var paras = ctor.GetParameters();
        if (paras.Length > args.Length)
        {
            args = args.Concat(paras.Skip(args.Length).Select(m => m.RawDefaultValue)).ToArray();
        }
        return ctor.Invoke(args);

        static bool ArgumentListMatches(MethodBase method, object?[] args)
        {
            var parameters = method.GetParameters();
            if (parameters.Length < args.Length)
                return false;

            for (var i = 0; i < args.Length; i++)
            {
                if (ParameterMatches(parameters[i].ParameterType, args[i]) == false)
                    return false;
            }

            return parameters.Skip(args.Length).All(p => p.IsOptional);
        }

        static bool ParameterMatches(Type parameterType, object? arg)
        {
            if (arg is null)
                return parameterType.IsValueType == false || Nullable.GetUnderlyingType(parameterType) != null;

            var argType = arg.GetType();
            if (parameterType.IsAssignableFrom(argType))
                return true;

            if (Nullable.GetUnderlyingType(parameterType) is { } underlyingType)
                return underlyingType.IsAssignableFrom(argType);

            return CanWidenPrimitive(argType, parameterType);
        }

        static bool CanWidenPrimitive(Type sourceType, Type targetType)
        {
            return Type.GetTypeCode(sourceType) switch
            {
                TypeCode.Char => Type.GetTypeCode(targetType) is TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.SByte => Type.GetTypeCode(targetType) is TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.Byte => Type.GetTypeCode(targetType) is TypeCode.Char or TypeCode.UInt16 or TypeCode.Int16 or TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.Int16 => Type.GetTypeCode(targetType) is TypeCode.Int32 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.UInt16 => Type.GetTypeCode(targetType) is TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.Int32 => Type.GetTypeCode(targetType) is TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.UInt32 => Type.GetTypeCode(targetType) is TypeCode.UInt64 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double,
                TypeCode.Int64 => Type.GetTypeCode(targetType) is TypeCode.Single or TypeCode.Double,
                TypeCode.UInt64 => Type.GetTypeCode(targetType) is TypeCode.Single or TypeCode.Double,
                TypeCode.Single => Type.GetTypeCode(targetType) is TypeCode.Double,
                _ => false,
            };
        }
    }

    /// <summary>
    /// Creates an instance of the specified type and casts it to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>The created object cast to <typeparamref name="T"/>.</returns>
    /// <inheritdoc cref="CreateObject(Type, object?[])"/>
    [MethodImpl(AggressiveInlining)]
    public static T CreateObject<T>(this Type type, params object?[] args)
    {
        return type.CreateObject(args).CastTo<T>();
    }

    /// <summary>
    /// Gets the single interface implemented by <paramref name="type"/> that matches
    /// <paramref name="interfaceType"/>, either as an exact match or as a closed construction
    /// of an open generic interface definition (e.g. <see cref="IEnumerable{T}"/>).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="interfaceType">
    /// The interface to look for. Can be a non-generic interface, a closed generic interface,
    /// or an open generic interface definition.
    /// </param>
    /// <returns>
    /// The matching interface type, or <see langword="null"/> if not implemented.
    /// </returns>
    /// <exception cref="AmbiguousMatchException">
    /// Thrown when <paramref name="type"/> implements multiple closed constructions of the
    /// same open generic interface (e.g. both <see cref="IComparable{T}"/> variants). Use
    /// <see cref="GetImplementedInterfaces"/> instead in that case.
    /// </exception>
    public static Type? GetImplementedInterface(this Type type, Type interfaceType)
    {
        var matches = type.GetImplementedInterfaces(interfaceType);
        return matches.Length switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new AmbiguousMatchException(
                $"Type '{type}' implements multiple variants of '{interfaceType}': " +
                $"{string.Join(", ", matches.Select(m => m.ToString()))}. " +
                $"Use {nameof(GetImplementedInterfaces)} to retrieve all matches.")
        };
    }

    /// <summary>
    /// Gets all interfaces implemented by <paramref name="type"/> that match
    /// <paramref name="interfaceType"/>, either as an exact match or as a closed construction
    /// of an open generic interface definition.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="interfaceType">
    /// The interface to look for. Can be a non-generic interface, a closed generic interface,
    /// or an open generic interface definition.
    /// </param>
    /// <returns>
    /// An array of matching interface types. Empty if not implemented.
    /// </returns>
    public static Type[] GetImplementedInterfaces(this Type type, Type interfaceType)
    {
        return type.GetInterfaces()
            .Where(x => x == interfaceType || x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType)
            .ToArray();
    }

    /// <summary>
    /// Determines whether the specified type implements a given interface.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to inspect.</param>
    /// <param name="interfaceType">The interface type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the type implements the specified interface; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method internally uses <see cref="GetImplementedInterface"/> to perform the check and also treats an
    /// interface as implementing itself.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static bool Implements(this Type type, Type interfaceType)
    {
        return interfaceType.IsInterface
               && (type == interfaceType || type.GetImplementedInterface(interfaceType) != null);
    }

    /// <summary>
    /// Determines whether the specified type inherits from a given base type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to inspect.</param>
    /// <param name="baseType">The base type to check against.</param>
    /// <returns>
    /// <see langword="true"/> if the type inherits from the specified base type; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method supports both direct inheritance and generic type definitions.
    /// It starts from <see cref="Type.BaseType"/>, so it does not treat <paramref name="type"/>
    /// itself as inheriting from <paramref name="baseType"/>.
    /// </remarks>
    public static bool Inherits(this Type type, Type baseType)
    {
        var t = type.BaseType;
        while (t != null)
        {
            if (t == baseType)
                return true;

            if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == baseType)
                return true;

            t = t.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Determines whether the type is marked with <see cref="CompilerGeneratedAttribute"/>.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> when the type is compiler-generated; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(AggressiveInlining)]
    public static bool IsCompilerGenerated(this Type type)
    {
        return type.IsDefined<CompilerGeneratedAttribute>();
    }

#if !NET5_0_OR_GREATER
    /// <summary>
    /// Gets the constructor whose parameters match the specified types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="bindingAttr">The binding flags used to search for the constructor.</param>
    /// <param name="types">The constructor parameter types.</param>
    /// <returns>The matching constructor, or <see langword="null"/> when no constructor is found.</returns>
    /// <remarks>This polyfills the shorter overload available on newer target frameworks.</remarks>
    [MethodImpl(AggressiveInlining)]
    public static ConstructorInfo? GetConstructor(this Type type, BindingFlags bindingAttr, Type[] types)
    {
        return type.GetConstructor(bindingAttr, null, types, null);
    }

    /// <summary>
    /// Determines whether the type can be assigned to the target type.
    /// </summary>
    /// <param name="type">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns><see langword="true"/> when <paramref name="targetType"/> is assignable from <paramref name="type"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>This polyfills <see cref="Type.IsAssignableTo(Type?)"/> on older target frameworks.</remarks>
    [MethodImpl(AggressiveInlining)]
    public static bool IsAssignableTo(this Type type, [NotNullWhen(true)] Type? targetType)
    {
        return targetType?.IsAssignableFrom(type) ?? false;
    }
#endif

    /// <summary>
    /// Gets the <see cref="TypeCode"/> for the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The type code returned by <see cref="Type.GetTypeCode(Type)"/>.</returns>
    [MethodImpl(AggressiveInlining)]
    public static TypeCode GetTypeCode(this Type type)
    {
        return Type.GetTypeCode(type);
    }

    /// <summary>
    /// Determines whether an implicit conversion operator exists from the source type to the target type.
    /// </summary>
    /// <param name="sourceType">The conversion source type.</param>
    /// <param name="targetType">The conversion target type.</param>
    /// <param name="declaringType">
    /// The type on which to search for the operator. When omitted, both <paramref name="sourceType"/> and
    /// <paramref name="targetType"/> are searched.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a public static <c>op_Implicit</c> method with the requested source and
    /// target types is found; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasImplicitConversionOperator(this Type sourceType, Type targetType, Type? declaringType = null)
    {
        var declaringTypes = declaringType is null
            ? [sourceType, targetType]
            : new[] { declaringType };

        return declaringTypes
            .Distinct()
            .SelectMany(m => m.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
            .Any(mi =>
            {
                var parameter = mi.GetParameters().FirstOrDefault();
                return parameter != null && parameter.ParameterType == sourceType;
            });

    }

    /// <summary>
    /// Finds a public static conversion operator method (either implicit or explicit) defined on the specified declaring type that converts from <paramref name="fromType"/> to <paramref name="toType"/>.
    /// </summary>
    /// <param name="declaringType">The type on which to search for the operator.</param>
    /// <param name="fromType">The source type of the conversion.</param>
    /// <param name="toType">The target type of the conversion.</param>
    /// <returns>The <see cref="MethodInfo"/> representing the conversion operator if found; otherwise, <see langword="null"/>.</returns>
    public static MethodInfo? FindConversionOperator(this Type declaringType, Type fromType, Type toType)
    {
        return declaringType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name is "op_Implicit" or "op_Explicit"
                && m.ReturnType == toType
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType.IsAssignableFrom(fromType));
    }

    extension(Type)
    {
        public static Type GetRequiredType(string name) => Type.GetType(name) ?? throw new InvalidOperationException($"Cannot find type '{name}'");

        public static Type? GetType(string name, string assemblyName) => Type.GetType($"{name}, {assemblyName}");

        public static Type GetRequiredType(string name, string assemblyName)
            => Type.GetType($"{name}, {assemblyName}") ?? throw new InvalidOperationException($"Cannot find type '{name}'");
    }
}
